using Serilog.Events;

namespace PaymentGateway.PhoneApp.Services.Logs;

public class LogEntry(string message, LogEventLevel level)
{
    public string Message { get; set; } = message;
    public LogEventLevel Level { get; set; } = level;
}