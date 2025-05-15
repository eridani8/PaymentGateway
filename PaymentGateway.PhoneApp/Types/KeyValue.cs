using LiteDB;

namespace PaymentGateway.PhoneApp.Types;

public class KeyValue
{
    [BsonId] public required ObjectId Id { get; set; }
    public required string Key { get; set; }
    public object? Value { get; set; }
}