using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PaymentGateway.Core.Configs;

namespace PaymentGateway.Api.Configuration;

public static class OpenTelemetryConfiguration
{
    public static void Configure(WebApplicationBuilder builder, OpenTelemetryConfig otlpConfig)
    {
        builder.Services.Configure<OpenTelemetryConfig>(builder.Configuration.GetSection(nameof(OpenTelemetryConfig)));
        
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(builder.Environment.ApplicationName)
            .AddEnvironmentVariableDetector();

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    // .AddEntityFrameworkCoreInstrumentation(options =>
                    // {
                    //     options.SetDbStatementForText = true;
                    // }) // TODO
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri($"{otlpConfig.Endpoint}/ingest/otlp/v1/traces");
                        options.Protocol = OtlpExportProtocol.HttpProtobuf; // TODO
                        options.Headers = $"X-Seq-ApiKey={otlpConfig.Token}";
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri($"{otlpConfig.Endpoint}/ingest/otlp/v1/metrics");
                        options.Protocol = OtlpExportProtocol.HttpProtobuf; // TODO
                        options.Headers = $"X-Seq-ApiKey={otlpConfig.Token}";
                    });
            });
    }
}