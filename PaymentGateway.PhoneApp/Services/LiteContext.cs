using LiteDB;
using PaymentGateway.PhoneApp.Services.Logs;

namespace PaymentGateway.PhoneApp.Services;

public class LiteContext
{
    public ILiteCollection<LogEntry> Logs { get; }
    
    public LiteContext()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "data.db");
        var connectionString = $"Filename={path};Password=jT5C!1>PWDp£$eBaO+b26'0(G4q>";
        var db = new LiteDatabase(connectionString);
        Logs = db.GetCollection<LogEntry>("logs");
    }
}