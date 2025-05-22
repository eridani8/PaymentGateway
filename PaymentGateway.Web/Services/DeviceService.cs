using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.Types;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class DeviceService(
    IHttpClientFactory httpClientFactory,
    ILogger<DeviceService> logger,
    JsonSerializerOptions jsonSerializerOptions,
    AuthenticationStateProvider authStateProvider)
    : ServiceBase(httpClientFactory, logger, jsonSerializerOptions), IDeviceService
{
    private const string apiEndpoint = "api/device";
    public async Task<DeviceTokenDto?> GenerateDeviceToken()
    {
        return await PostRequest<DeviceTokenDto>($"{apiEndpoint}/user/token");
    }

    public async Task<List<DeviceDto>> GetOnlineDevices()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var isAdmin = authState.User.IsInRole("Admin");
        
        if (!isAdmin)
        {
            return await GetUserOnlineDevices();
        }
        
        var response = await GetRequest($"{apiEndpoint}");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<DeviceDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }

    public async Task<List<DeviceDto>> GetUserOnlineDevices()
    {
        var response = await GetRequest($"{apiEndpoint}/user");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<DeviceDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }

    public async Task<List<DeviceDto>> GetDevicesByUserId(Guid userId)
    {
        var response = await GetRequest($"{apiEndpoint}/user/{userId}");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<DeviceDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }
}