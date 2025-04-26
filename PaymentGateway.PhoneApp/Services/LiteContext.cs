using LiteDB;
using PaymentGateway.PhoneApp.Services.Logs;

namespace PaymentGateway.PhoneApp.Services;

public class LiteContext
{
    public ILiteCollection<LogEntry> Logs { get; }
    
    public LiteContext(AppSettings settings)
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "data.db");
        var connectionString = $"Filename={path};Password={settings.DbPassword}";
        var db = new LiteDatabase(connectionString);
        Logs = db.GetCollection<LogEntry>("logs");
    }
}