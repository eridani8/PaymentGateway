using LiteDB;

namespace PaymentGateway.PhoneApp.Types;

public class KeyValue
{
    [BsonId] public required ObjectId Id { get; init; }
    public required string Key { get; init; }
    public required object Value { get; init; }
}