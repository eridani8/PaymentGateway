using System.Collections.Concurrent;

namespace PaymentGateway.Application.Types;

public class OnlineDevices
{
    public ConcurrentDictionary<Guid, DeviceState> All { get; } = new();
}