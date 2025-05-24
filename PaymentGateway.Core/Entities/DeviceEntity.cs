using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Core.Entities;

public class DeviceEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }
    [MaxLength(255)] public required string DeviceName { get; set; }
    public DateTime BindingAt { get; set; }
    public Guid? RequisiteId { get; set; }
    public RequisiteEntity? Requisite { get; set; }

    public void ClearBinding()
    {
        BindingAt = DateTime.MinValue;
        RequisiteId = null;
        Requisite = null;
    }
    
    public void SetBinding(Guid requisiteId)
    {
        BindingAt = DateTime.UtcNow;
        RequisiteId = requisiteId;
    }
}