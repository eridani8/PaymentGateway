using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
using UserStatusFilter = PaymentGateway.Api.Filters.UserStatusFilter;
using PaymentGateway.Shared.Interfaces;
using Microsoft.AspNetCore.SignalR;

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

    builder.Host.UseSerilog(Log.Logger);

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<UserStatusFilter>();
    });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(o =>
    {
        o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization",
            Type = SecuritySchemeType.Http,
            Name = "Authorization",
            In = ParameterLocation.Header,
            Scheme = "Bearer",
            BearerFormat = "JWT",
        });

        o.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    });

    builder.Services.AddCore(builder.Configuration);
    builder.Services.AddInfrastructure(connectionString);
    builder.Services.AddApplication();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddSignalR(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(60);
        options.KeepAliveInterval = TimeSpan.FromMinutes(1);
        options.HandshakeTimeout = TimeSpan.FromMinutes(1);
        options.MaximumReceiveMessageSize = 1024 * 1024;
        options.EnableDetailedErrors = true;
        options.StreamBufferCapacity = 20;
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

    // if (app.Environment.IsDevelopment()) // TODO
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowAll"); // TODO

    // app.UseHttpsRedirection(); // TODO
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseExceptionHandler();

    app.MapHub<NotificationHub>("/notificationHub");
    
    using (var scope = app.Services.CreateScope())
    {
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub, IHubClient>>();
        NotificationHub.Initialize(hubContext);
    }

    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var roles = new[] { "User", "Admin", "Support" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();

        var rootUser = await userManager.FindByNameAsync("root");
        if (rootUser == null)
        {
            rootUser = new UserEntity
            {
                Id = Guid.CreateVersion7(),
                UserName = "root",
                IsActive = true,
                MaxRequisitesCount = int.MaxValue,
                MaxDailyMoneyReceptionLimit = 999_999_999_999.99m
            };
            var result = await userManager.CreateAsync(rootUser, "Qwerty123_");
            if (!result.Succeeded)
            {
                throw new ApplicationException("Ошибка при создании root пользователя");
            }

            foreach (var role in roles)
            {
                if (!await userManager.IsInRoleAsync(rootUser, role))
                {
                    await userManager.AddToRoleAsync(rootUser, role);
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