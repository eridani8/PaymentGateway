using LiteDB;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Types;

namespace PaymentGateway.PhoneApp.Services;

public class LiteContext
{
    public Guid DeviceId { get; } = Guid.Empty;
    public ILiteCollection<KeyValue> KeyValues { get; }
    public ILiteCollection<LogEntry> Logs { get; }

    public LiteContext(AppSettings settings)
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "data.db");
        var connectionString = $"Filename={path};Password={settings.DbPassword}";
        var db = new LiteDatabase(connectionString);
        Logs = db.GetCollection<LogEntry>("logs");
        KeyValues = db.GetCollection<KeyValue>("key_values");

        if (DeviceId == Guid.Empty)
        {
            if (KeyValues.FindOne(k => k.Key == "DeviceId") is not { } keyValue)
            {
                keyValue = new KeyValue()
                {
                    Id = ObjectId.NewObjectId(),
                    Key = "DeviceId",
                    Value = Guid.CreateVersion7()
                };
                KeyValues.Insert(keyValue);
            }

            if (keyValue.Value is Guid guid)
            {
                DeviceId = guid;
            }
            else
            {
                DeviceId = Guid.Empty;
            }
        }
    }
}