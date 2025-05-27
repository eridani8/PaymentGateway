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
    
    public async Task<Response> DeleteDevice(Guid id)
    {
        return await DeleteRequest($"{apiEndpoint}/{id}");
    }
    
    public async Task<List<DeviceDto>> GetDevices()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var isAdmin = authState.User.IsInRole("Admin");
        
        if (!isAdmin)
        {
            return await GetUserDevices();
        }
        
        var response = await GetRequest($"{apiEndpoint}");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<DeviceDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }

    public async Task<List<DeviceDto>> GetUserDevices(bool onlyAvailable = false, bool onlyOnline = false)
    {
        var queryParams = new List<string>();
        if (onlyAvailable) queryParams.Add("onlyAvailable=true");
        if (onlyOnline) queryParams.Add("onlyOnline=true");
        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        
        var response = await GetRequest($"{apiEndpoint}/user{query}");
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