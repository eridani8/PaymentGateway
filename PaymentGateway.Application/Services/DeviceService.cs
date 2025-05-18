using System.Collections.Concurrent;
using AutoMapper;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.Device;

namespace PaymentGateway.Application.Services;

public class DeviceService(OnlineDevices devices) : IDeviceService
{
    public List<DeviceDto> GetOnlineDevices()
    {
        return devices.All.Values.ToList();
    }
}

public class OnlineDevices
{
    public ConcurrentDictionary<Guid, DeviceDto> All { get; } = new();
}