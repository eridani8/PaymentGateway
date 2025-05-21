using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Core.Entities;

public class DeviceEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }
    public Guid DeviceId { get; set; }
    [MaxLength(100)] public required string DeviceData { get; set; }
    public DateTime BindingAt { get; set; }
}