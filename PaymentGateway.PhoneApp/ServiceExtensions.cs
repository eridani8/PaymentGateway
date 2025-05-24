using System.Reflection;
using PaymentGateway.PhoneApp.Parsers;

namespace PaymentGateway.PhoneApp;

public static class ServiceExtensions
{
    public static IServiceCollection AddParsers(this IServiceCollection services)
    {
        services.AddParsers<ISmsParser>();
        services.AddParsers<INotificationParser>();
        services.AddSingleton<ParserFactory>();

        return services;
    }

    public static IServiceCollection AddParsers<T>(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var parserTypes = assembly.GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                typeof(T).IsAssignableFrom(t));

        foreach (var type in parserTypes)
        {
            services.AddSingleton(type);
        }

        return services;
    }
}