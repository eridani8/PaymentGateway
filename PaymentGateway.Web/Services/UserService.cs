using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Interfaces;
using System.Text.Json;
using System.Net;

namespace PaymentGateway.Web.Services;

public class UserService(
    IHttpClientFactory factory,
    ILogger<UserService> logger,
    JsonSerializerOptions jsonOptions) : ServiceBase(factory, logger, jsonOptions), IUserService
{
    private const string ApiEndpoint = "user";
    
    public async Task<Response> Login(LoginDto dto)
    {
        var response = await PostRequest($"{ApiEndpoint}/login", dto);
        var result = new Response
        {
            Code = response.Code,
            Content = response.Content,
        };

        return result;
    }

    public Task<Response> ChangePasswordAsync(ChangePasswordDto dto)
    {
        return PostRequest($"{ApiEndpoint}/ChangePassword", dto);
    }

    public async Task<Response<TwoFactorStatusDto>> GetTwoFactorStatusAsync()
    {
        var response = await GetRequest($"{ApiEndpoint}/TwoFactorStatus");
        var result = new Response<TwoFactorStatusDto>
        {
            Code = response.Code,
            Content = response.Content
        };

        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            result.Data = JsonSerializer.Deserialize<TwoFactorStatusDto>(response.Content, JsonOptions);
        }
        else
        {
            logger.LogWarning("Failed to get two-factor status. Status code: {StatusCode}", response.Code);
        }

        return result;
    }
    
    public async Task<Response<TwoFactorDto>> EnableTwoFactorAsync()
    {
        var response = await PostRequest($"{ApiEndpoint}/EnableTwoFactor", new {});
        var result = new Response<TwoFactorDto>
        {
            Code = response.Code,
            Content = response.Content
        };

        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            result.Data = JsonSerializer.Deserialize<TwoFactorDto>(response.Content, JsonOptions);
        }
        else
        {
            logger.LogWarning("Failed to enable two-factor authentication. Status code: {StatusCode}", response.Code);
        }

        return result;
    }

    public async Task<Response> VerifyTwoFactorAsync(TwoFactorVerifyDto dto)
    {
        return await PostRequest($"{ApiEndpoint}/VerifyTwoFactor", dto);
    }
}