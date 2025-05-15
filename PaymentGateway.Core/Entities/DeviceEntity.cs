namespace PaymentGateway.Core.Entities;

public class DeviceEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid DeviceId { get; set; }
    public DateTime BindingAt { get; set; }
}