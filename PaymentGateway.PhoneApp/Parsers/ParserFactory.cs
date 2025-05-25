namespace PaymentGateway.PhoneApp.Parsers;

public class ParserFactory(IServiceProvider serviceProvider)
{
    public ISmsParser? GetSmsParser(string number)
    {
        var parsers = serviceProvider.GetServices<ISmsParser>().ToList();
        return parsers.FirstOrDefault(p => p.Numbers.Any(n => n.Equals(number, StringComparison.OrdinalIgnoreCase)));
    }

    public INotificationParser? GetNotificationParser(string appName)
    {
        var parsers = serviceProvider.GetServices<INotificationParser>().ToList();
        return parsers.FirstOrDefault(p => p.AppName == appName);
    }
    
    public IEnumerable<ISmsParser> GetSmsParsers()
    {
        return serviceProvider.GetServices<ISmsParser>();
    }
    
    public IEnumerable<INotificationParser> GetNotificationParsers()
    {
        return serviceProvider.GetServices<INotificationParser>();
    }
}