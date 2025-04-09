namespace PaymentGateway.Core;

public class CacheMetadata
{
    public DateTime? ExpiryTime { get; set; }
    public TimeSpan? OriginalExpiry { get; set; }
}