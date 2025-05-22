using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Core.Entities;

public class DeviceEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }
    [MaxLength(255)] public required string DeviceName { get; set; }
    public DateTime BindingAt { get; set; }
}