using NpgsqlTypes;
using PaymentGateway.Core;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace PaymentGateway.Api.Configuration;

public static class LoggingConfiguration
{
    public static Serilog.ILogger ConfigureLogger(string connectionString, OpenTelemetryConfig otlpConfig)
    {
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
        return new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("ApplicationName", "PaymentGateway")
            .WriteTo.Console(outputTemplate: outputTemplate, levelSwitch: levelSwitch)
            .WriteTo.PostgreSQL(connectionString, logs, columnWriters, needAutoCreateTable: true, levelSwitch: levelSwitch)
            .WriteTo.File($"{logsPath}/.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate, levelSwitch: levelSwitch)
            // .WriteTo.OpenTelemetry(options =>
            // {
            //     options.Endpoint = otlpConfig.Endpoint;
            //     options.ResourceAttributes = new Dictionary<string, object>
            //     {
            //         ["service.name"] = "PaymentGateway",
            //     };
            //     options.LevelSwitch = levelSwitch;
            // })
            .WriteTo.Seq(otlpConfig.Endpoint)
            .CreateLogger();
    }
} 