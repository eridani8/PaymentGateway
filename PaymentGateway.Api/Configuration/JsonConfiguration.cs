using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Configuration;

public static class JsonConfiguration
{
    public static void ConfigureJson(WebApplicationBuilder builder)
    {
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
    }
} 