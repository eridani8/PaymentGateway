namespace PaymentGateway.Core;

public class CacheMetadata
{
    public DateTime? ExpiryTime { get; init; }
    public TimeSpan? OriginalExpiry { get; init; }
}