using System.Collections.ObjectModel;
using LiteDB;
using PaymentGateway.PhoneApp.Services;
using Serilog.Core;
using Serilog.Events;

namespace PaymentGateway.PhoneApp.Types;

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
        
        string? sourceContext = null;
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContextProperty) && 
            sourceContextProperty is ScalarValue { Value: string sourceContextValue })
        {
            sourceContext = sourceContextValue;
        }
        
        var logEntry = new LogEntry()
        {
            Id = ObjectId.NewObjectId(),
            Timestamp = logEvent.Timestamp.LocalDateTime,
            Level = logEvent.Level,
            Message = message,
            Context = sourceContext
        };
        _context.Logs.Insert(logEntry);
        Logs.Add(logEntry);
        LogAdded?.Invoke(this, logEntry);
    }
}