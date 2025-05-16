using System.Collections.Concurrent;

namespace PaymentGateway.Application.Types;

public class AvailableDevices
{
    public ConcurrentDictionary<Guid, DeviceState> Devices { get; } = new();
}