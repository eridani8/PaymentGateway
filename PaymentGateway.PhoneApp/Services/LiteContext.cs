using LiteDB;
using PaymentGateway.PhoneApp.Types;

namespace PaymentGateway.PhoneApp.Services;

public class LiteContext
{
    public ILiteCollection<KeyValue> KeyValues { get; }
    public ILiteCollection<LogEntry> Logs { get; }
    
    public LiteContext(AppSettings settings)
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "data.db");
        var connectionString = $"Filename={path};Password={settings.DbPassword}";
        var db = new LiteDatabase(connectionString);
        Logs = db.GetCollection<LogEntry>("logs");
        KeyValues = db.GetCollection<KeyValue>("key_values");
    }
}