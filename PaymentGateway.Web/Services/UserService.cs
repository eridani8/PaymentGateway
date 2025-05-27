using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Interfaces;
using System.Text.Json;
using System.Net;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.Web.Services;

public class UserService(
    IHttpClientFactory factory,
    ILogger<UserService> logger,
    JsonSerializerOptions jsonOptions) : ServiceBase(factory, logger, jsonOptions), IUserService
{
    private const string apiEndpoint = "api/users";

    public async Task<WalletDto?> GetWalletState()
    {
        var response = await GetRequest($"{apiEndpoint}/wallet");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<WalletDto>(response.Content, JsonOptions);
        }
        return null;
    }
    
    public async Task<Response> Login(LoginDto dto)
    {
        return await PostRequest($"{apiEndpoint}/login", dto);
    }

    public Task<Response> ChangePassword(ChangePasswordDto dto)
    {
        return PutRequest($"{apiEndpoint}/password", dto);
    }

    public async Task<Response<TwoFactorStatusDto>> GetTwoFactorStatus()
    {
        var response = await GetRequest($"{apiEndpoint}/two-factor/status");
        var result = new Response<TwoFactorStatusDto>
        {
            Code = response.Code,
            Content = response.Content
        };

        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            result.Data = JsonSerializer.Deserialize<TwoFactorStatusDto>(response.Content, JsonOptions);
        }

        return result;
    }

    public async Task<Response<TwoFactorDto>> EnableTwoFactor()
    {
        var response = await PostRequest($"{apiEndpoint}/two-factor/enable");
        var result = new Response<TwoFactorDto>
        {
            Code = response.Code,
            Content = response.Content
        };

        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            result.Data = JsonSerializer.Deserialize<TwoFactorDto>(response.Content, JsonOptions);
        }

        return result;
    }

    public async Task<Response> VerifyTwoFactor(TwoFactorVerifyDto dto)
    {
        return await PostRequest($"{apiEndpoint}/two-factor/verify", dto);
    }
}