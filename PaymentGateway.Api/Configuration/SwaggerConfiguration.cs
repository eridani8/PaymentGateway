using Asp.Versioning.ApiExplorer;
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
            var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
            
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo
                    {
                        Title = "Payment Gateway API",
                        Version = description.ApiVersion.ToString(),
                        Description = "API для взаимодействия с платежным шлюзом",
                        Contact = new OpenApiContact
                        {
                            Name = "Support",
                            Email = "support@example.com"
                        }
                    });
            }

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