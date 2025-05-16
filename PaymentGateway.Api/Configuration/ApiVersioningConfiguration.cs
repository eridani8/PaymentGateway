using Asp.Versioning;

namespace PaymentGateway.Api.Configuration;

public static class ApiVersioningConfiguration
{
    public static void ConfigureApiVersioning(WebApplicationBuilder builder)
    {
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;

            options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
        });
    }
} 