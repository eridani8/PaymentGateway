using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Core.Entities;

public class ChatMessageEntity : ICacheable
{
    public Guid Id { get; init; }
    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}