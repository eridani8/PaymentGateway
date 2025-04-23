using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Core.Entities;

public class ChatMessageEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}