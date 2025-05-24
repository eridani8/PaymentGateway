using System.Text.Json.Serialization;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.DTOs.Device;

public class DeviceDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; set; }
    public UserDto? User { get; set; }
    public bool State { get; set; }
    public required string DeviceName { get; set; }
    public DateTime BindingAt { get; set; }
    [JsonIgnore] public string? ConnectionId { get; set; }
    public Guid? RequisiteId { get; set; }

    public void ClearBinding()
    {
        BindingAt = DateTime.MinValue;
        RequisiteId = null;
    }
    
    public void SetBinding(Guid requisiteId)
    {
        BindingAt = DateTime.UtcNow;
        RequisiteId = requisiteId;
    }
}