namespace PaymentGateway.Api.Configuration;

public static class CorsConfiguration
{
    public static void ConfigureCors(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }
} 