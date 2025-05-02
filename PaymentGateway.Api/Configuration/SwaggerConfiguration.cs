using Microsoft.OpenApi.Models;

namespace PaymentGateway.Api.Configuration;

public static class SwaggerConfiguration
{
    public static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Payment Gateway API",
                Version = "v1",
                Description = "API для взаимодействия с платежным шлюзом",
                Contact = new OpenApiContact
                {
                    Name = "Support",
                    Email = "support@example.com"
                }
            });

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
                    new List<string>()
                }
            });
            
            options.EnableAnnotations();
        });
    }
} 