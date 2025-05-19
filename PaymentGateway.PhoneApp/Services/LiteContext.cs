using LiteDB;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Types;

namespace PaymentGateway.PhoneApp.Services;

public class LiteContext
{
    private const string deviceIdKey = "DeviceId";
    public const string tokenKey = "Token";
    
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

    public Guid GetDeviceId()
    {
        if (KeyValues.FindOne(k => k.Key == deviceIdKey) is not { } deviceIdValue)
        {
            deviceIdValue = new KeyValue()
            {
                Id = ObjectId.NewObjectId(),
                Key = "DeviceId",
                Value = Guid.CreateVersion7()
            };
            KeyValues.Insert(deviceIdValue);
        }

        if (deviceIdValue.Value is Guid guid)
        {
            return guid;
        }

        return Guid.Empty;
    }

    public string GetToken()
    {
        if (KeyValues.FindOne(k => k.Key == tokenKey) is { Value: string token })
        {
            return token;
        }
        
        return string.Empty;
    }
}