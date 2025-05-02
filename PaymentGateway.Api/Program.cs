using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NpgsqlTypes;
using PaymentGateway.Api;
using PaymentGateway.Application;
using PaymentGateway.Application.Hubs;
using PaymentGateway.Application.Services;
using PaymentGateway.Core;
using PaymentGateway.Core.Entities;
using PaymentGateway.Infrastructure;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using PaymentGateway.Shared.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.OpenApi.Models;
using PaymentGateway.Api.Endpoints;

try
{
    var builder = WebApplication.CreateBuilder(args);

    var connectionString = builder.Configuration.GetConnectionString("Default");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ApplicationException("Нужно указать строку подключения базы данных");
    }

    var authConfig = builder.Configuration.GetSection(nameof(AuthConfig)).Get<AuthConfig>();
    if (authConfig is null)
    {
        throw new ApplicationException("Нужно указать настройки аутентификации");
    }

    #region Create Logger

    const string logs = "Logs";
    var logsPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logs));

    if (!Directory.Exists(logsPath))
    {
        Directory.CreateDirectory(logsPath);
    }

    var columnWriters = new Dictionary<string, ColumnWriterBase>
    {
        { "raise_date", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
        { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
        { "source_context", new SinglePropertyColumnWriter("SourceContext", PropertyWriteMethod.Raw) },
        { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
        { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) }
    };

    const string outputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    var levelSwitch = new LoggingLevelSwitch();
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.ControlledBy(levelSwitch)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: outputTemplate, levelSwitch: levelSwitch)
        .WriteTo.PostgreSQL(connectionString, logs, columnWriters, needAutoCreateTable: true, levelSwitch: levelSwitch)
        .WriteTo.File($"{logsPath}/.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate,
            levelSwitch: levelSwitch)
        .CreateLogger();

    #endregion

    builder.Services.Configure<GatewaySettings>(builder.Configuration.GetSection(nameof(GatewaySettings)));

    builder.Host.UseSerilog(Log.Logger);

    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;

        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    });

    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization",
            Type = SecuritySchemeType.Http,
            Name = "Authorization",
            In = ParameterLocation.Header,
            Scheme = "Bearer",
            BearerFormat = "JWT",
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                []
            }
        });
        
        options.EnableAnnotations();
    });

    builder.Services.AddCore(builder.Configuration);
    builder.Services.AddInfrastructure(connectionString);
    builder.Services.AddApplication();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var jsonOptions = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true
    };
    
    builder.Services.AddSingleton(jsonOptions);

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.SerializerOptions.PropertyNameCaseInsensitive = true;
    });

    builder.Services.AddSignalR(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(60);
        options.KeepAliveInterval = TimeSpan.FromMinutes(1);
        options.HandshakeTimeout = TimeSpan.FromMinutes(1);
        options.MaximumReceiveMessageSize = 1024 * 1024;
        options.EnableDetailedErrors = true;
        options.StreamBufferCapacity = 20;
    })
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
    })
    .AddMessagePackProtocol();
    builder.Services.AddScoped<INotificationService, NotificationService>();

    var secretKey = authConfig.SecretKey;
    var key = Encoding.ASCII.GetBytes(secretKey);
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = authConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = authConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<UserEntity>>();
                    var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    
                    if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out _))
                    {
                        context.Fail("Неверный идентификатор пользователя");
                        return;
                    }

                    var user = await userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        context.Fail("Пользователь не найден");
                        return;
                    }

                    if (!user.IsActive)
                    {
                        context.Fail("Пользователь деактивирован");
                        return;
                    }
                }
            };
        });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    }); // TODO

    var app = builder.Build();

    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("AllowAll"); // TODO

    // app.UseHttpsRedirection(); // TODO
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseExceptionHandler();

    app.MapHub<NotificationHub>("/notificationHub");
    
    HealthEndpoints.MapHealthEndpoints(app);
    UsersEndpoints.MapUsersEndpoints(app);
    AdminEndpoints.MapAdminEndpoints(app);
    RequisitesEndpoints.MapRequisitesEndpoints(app);
    PaymentsEndpoints.MapPaymentsEndpoints(app);
    TransactionsEndpoints.MapTransactionsEndpoints(app);
    
    using (var scope = app.Services.CreateScope())
    {
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub, IHubClient>>();
        NotificationHub.Initialize(hubContext);
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();

        var roles = new[] { "Admin", "User", "Support" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        await CreateUser("root");
        await CreateUser("eridani");

        async Task CreateUser(string username)
        {
            
            var defaultUser = await userManager.FindByNameAsync(username);
            if (defaultUser == null)
            {
                defaultUser = new UserEntity
                {
                    Id = Guid.CreateVersion7(),
                    UserName = username,
                    IsActive = true,
                    MaxRequisitesCount = int.MaxValue,
                    MaxDailyMoneyReceptionLimit = 9999999999999999m,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(defaultUser, "Qwerty123_");
                if (!result.Succeeded)
                {
                    throw new ApplicationException($"Ошибка при создании пользователя {username}");
                }

                foreach (var role in roles)
                {
                    if (!await userManager.IsInRoleAsync(defaultUser, role))
                    {
                        await userManager.AddToRoleAsync(defaultUser, role);
                    }
                }
            }
        }
    }

    await app.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Сервис не смог запуститься");
}
finally
{
    await Log.CloseAndFlushAsync();
}