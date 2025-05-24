using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PaymentGateway.Core.Configs;
using PaymentGateway.Core.Encryption;
using PaymentGateway.Shared;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../PaymentGateway.Api");
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("Default"));
        
        var cryptoConfigSection = config.GetSection("CryptographyConfig");
        var cryptoConfig = cryptoConfigSection.Get<CryptographyConfig>();
        var options = Options.Create(cryptoConfig!);
        var cryptographyService = new CryptographyService(options);

        return new AppDbContext(optionsBuilder.Options, cryptographyService);
    }
}