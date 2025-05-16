using System.Globalization;
using LiteDB;
using Serilog.Events;

namespace PaymentGateway.PhoneApp.Types;

public class LogEntry()
{
    [BsonId] public required ObjectId Id { get; init; }
    public DateTime Timestamp { get; init; }
    public LogEventLevel Level { get; init; }
    public required string Message { get; init; }
    public string? Context { get; init; }
    public string AsString => $"[{Timestamp.ToString(CultureInfo.InvariantCulture)}] {Level.ToString()}{Environment.NewLine}{Message}";
    public string ExportString => $"[{Timestamp.ToString(CultureInfo.InvariantCulture)}] [{Context}] {Level.ToString()}{(Context != null ? $" [{Context}]" : "")}{Environment.NewLine}{Message}";
}