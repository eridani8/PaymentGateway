using System.Collections.ObjectModel;
using Serilog.Core;
using Serilog.Events;

namespace PaymentGateway.PhoneApp.Services.Logs;

public class InMemoryLogSink : ILogEventSink
{
    public ObservableCollection<LogEntry> Logs { get; } = [];
    
    public event EventHandler<LogEntry>? LogAdded;
    
    public void Emit(LogEvent logEvent)
    {
        var timestamp = logEvent.Timestamp.LocalDateTime.ToString("HH:mm:ss");
        var level = logEvent.Level.ToString();
        var exception = logEvent.Exception != null ? Environment.NewLine + logEvent.Exception : "";
        
        var message = $"[{timestamp} {level}]{Environment.NewLine}{logEvent.RenderMessage()}{exception}";
        var logEntry = new LogEntry(message, logEvent.Level);
        
        Logs.Add(logEntry);
        LogAdded?.Invoke(this, logEntry);
    }
}