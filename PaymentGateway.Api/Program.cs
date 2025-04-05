using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using NpgsqlTypes;
using PaymentGateway.Api;
using PaymentGateway.Application.DTOs.Payment;
using PaymentGateway.Application.DTOs.Requisite;
using PaymentGateway.Application.DTOs.Transaction;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Mappings;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Validators.Payment;
using PaymentGateway.Application.Validators.Requisite;
using PaymentGateway.Application.Validators.Transaction;
using PaymentGateway.Core;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Repositories;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

try
{
    var builder = WebApplication.CreateBuilder(args);

    const string logs = "logs";
    var logsPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logs));

    if (!Directory.Exists(logsPath))
    {
        Directory.CreateDirectory(logsPath);
    }

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ApplicationException("Нужно указать строку подключения базы данных");
    }

    var columnWriters = new Dictionary<string, ColumnWriterBase>
    {
        { "raise_date", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
        { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
        { "source_context", new SinglePropertyColumnWriter("SourceContext", PropertyWriteMethod.Raw) },
        { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
        { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) }
    };

    const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(LogEventLevel.Information, outputTemplate: outputTemplate)
        .WriteTo.PostgreSQL(connectionString, logs, columnWriters, needAutoCreateTable: true)
        .WriteTo.File($"{logsPath}/.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate)
        .CreateLogger();

    builder.Host.UseSerilog(Log.Logger);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<AppDbContext>(o =>
    {
        o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
    
    builder.Services.Configure<RequisiteDefaults>(builder.Configuration.GetSection(nameof(RequisiteDefaults)));
    builder.Services.Configure<PaymentDefaults>(builder.Configuration.GetSection(nameof(PaymentDefaults)));
    builder.Services.Configure<CryptographyConfig>(builder.Configuration.GetSection(nameof(CryptographyConfig)));
    
    builder.Services.AddScoped<ICryptographyService, CryptographyServiceService>();

    builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
    builder.Services.AddScoped<IRequisiteRepository, RequisiteRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    
    builder.Services.AddAutoMapper(typeof(RequisiteProfile));
    builder.Services.AddAutoMapper(typeof(PaymentProfile));
    builder.Services.AddAutoMapper(typeof(TransactionProfile));

    builder.Services.AddScoped<IValidator<RequisiteCreateDto>, RequisiteCreateDtoValidator>();
    builder.Services.AddScoped<IValidator<RequisiteUpdateDto>, RequisiteUpdateDtoValidator>();
    builder.Services.AddScoped<IRequisiteValidator, RequisiteValidator>();
    builder.Services.AddScoped<IRequisiteService, RequisiteService>();

    builder.Services.AddScoped<IValidator<PaymentCreateDto>, PaymentCreateDtoValidator>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();

    builder.Services.AddScoped<IValidator<TransactionCreateDto>, TransactionCreateDtoValidator>();
    builder.Services.AddScoped<ITransactionService, TransactionService>();

    builder.Services.AddScoped<IExpiredPaymentHandler, ExpiredPaymentHandler>();
    builder.Services.AddScoped<IUnprocessedPaymentHandler, UnprocessedPaymentHandler>();
    
    builder.Services.AddHostedService<PaymentHandler>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.MapControllers();
    app.UseMiddleware<ExceptionHandling>();

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