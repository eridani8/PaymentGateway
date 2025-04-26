using System.Collections.ObjectModel;
using LiteDB;
using Serilog.Core;
using Serilog.Events;

namespace PaymentGateway.PhoneApp.Services.Logs;

public class InMemoryLogSink : ILogEventSink
{
    private readonly LiteContext _context;
    public ObservableCollection<LogEntry> Logs { get; } = [];

    public event EventHandler<LogEntry>? LogAdded;

    public InMemoryLogSink(LiteContext context)
    {
        _context = context;
        LoadLogs();
    }

    private void LoadLogs()
    {
        var logs = _context.Logs
            .FindAll()
            .ToList();
        foreach (var log in logs)
        {
            Logs.Add(log);
        }
    }

    public void Emit(LogEvent logEvent)
    {
        var exception = logEvent.Exception != null ? Environment.NewLine + logEvent.Exception : "";
        var message = $"{logEvent.RenderMessage()}{exception}";
        var logEntry = new LogEntry()
        {
            Id = ObjectId.NewObjectId(),
            Timestamp = logEvent.Timestamp.LocalDateTime,
            Level = logEvent.Level,
            Message = message
        };
        _context.Logs.Insert(logEntry);
        Logs.Add(logEntry);
        LogAdded?.Invoke(this, logEntry);
    }
}