using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using PaymentGateway.Api;
using PaymentGateway.Application.DTOs;
using PaymentGateway.Application.Mappings;
using PaymentGateway.Application.Services;
using PaymentGateway.Application.Validators.Requisite;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Repositories;
using Serilog;
using Serilog.Events;
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
        { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
        { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) }
    };

    var logger = Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(LogEventLevel.Information)
        .WriteTo.PostgreSQL(connectionString, logs, columnWriters, needAutoCreateTable: true)
        .WriteTo.File($"{logsPath}/.log", rollingInterval: RollingInterval.Day)
        .CreateLogger();

    builder.Host.UseSerilog(logger);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<AppDbContext>(o =>
    {
        o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        o.UseLoggerFactory(LoggerFactory.Create(b => b.AddSerilog()));
    });

    builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
    builder.Services.AddScoped<IRequisiteRepository, RequisiteRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    
    builder.Services.AddAutoMapper(typeof(RequisiteProfile));

    builder.Services.AddScoped<IValidator<RequisiteCreateDto>, RequisiteCreateDtoValidator>();
    builder.Services.AddScoped<IValidator<RequisiteUpdateDto>, RequisiteUpdateDtoValidator>();
    builder.Services.AddScoped<RequisiteValidator>();
    builder.Services.AddScoped<RequisiteService>();

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
    Log.Fatal(e, "Сервер не смог запуститься");
}
finally
{
    await Log.CloseAndFlushAsync();
}