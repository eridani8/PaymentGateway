using System.Globalization;
using System.Text;
using LiteDB;
using Serilog.Events;

namespace PaymentGateway.PhoneApp.Services.Logs;

public class LogEntry()
{
    [BsonId] public required ObjectId Id { get; init; }
    public DateTime Timestamp { get; init; }
    public LogEventLevel Level { get; init; }
    public required string Message { get; init; }
    public string AsString => $"[{Timestamp.ToString(CultureInfo.CurrentCulture)}] {Level.ToString()}{Environment.NewLine}{Message}";
}