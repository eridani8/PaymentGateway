using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.PhoneApp.Interfaces;

public interface IDeviceNotificationService
{
    Task InitializeAsync();
} 