using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Types;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class AdminService(
    IHttpClientFactory factory,
    ILogger<AdminService> logger,
    JsonSerializerOptions jsonOptions,
    AuthenticationStateProvider authStateProvider) : ServiceBase(factory, logger, jsonOptions), IAdminService
{
    private const string apiEndpoint = "api/admin";

    public async Task<List<UserDto>> GetAllUsers()
    {
        var response = await GetRequest($"{apiEndpoint}/users");
        if (response.Code == HttpStatusCode.OK)
        {
            return JsonSerializer.Deserialize<List<UserDto>>(response.Content ?? string.Empty, JsonOptions) ?? [];
        }

        return [];
    }

    public async Task<Response> CreateUser(CreateUserDto dto)
    {
        return await PostRequest($"{apiEndpoint}/users", dto);
    }

    public async Task<UserDto?> GetUserById(Guid id)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var isAdmin = authState.User.IsInRole("Admin") || authState.User.IsInRole("Support");

        if (!isAdmin)
        {
            logger.LogWarning("Non-admin user attempted to access user data for user ID: {UserId}", id);
            return null;
        }

        var response = await GetRequest($"{apiEndpoint}/users/{id}");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<UserDto>(response.Content, JsonOptions);
        }

        return null;
    }

    public async Task<Response> DeleteUser(Guid id)
    {
        return await DeleteRequest($"{apiEndpoint}/users/{id}");
    }

    public async Task<Response> UpdateUser(UpdateUserDto dto)
    {
        return await PutRequest($"{apiEndpoint}/users", dto);
    }

    public async Task<Dictionary<Guid, string>> GetUsersRoles(List<Guid> userIds)
    {
        try
        {
            var response = await GetRequest($"{apiEndpoint}/users/roles?userIds={string.Join(",", userIds)}");
            if (response.Code == HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<Dictionary<Guid, string>>(response.Content ?? string.Empty,
                    JsonOptions) ?? [];
            }

            return [];
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении ролей пользователей");
            throw;
        }
    }

    public async Task<Response> ResetTwoFactor(Guid userId)
    {
        return await PutRequest($"{apiEndpoint}/users/{userId}/reset-2fa");
    }

    public async Task<RequisiteAssignmentAlgorithm> GetCurrentRequisiteAssignmentAlgorithm()
    {
        var response = await GetRequest($"{apiEndpoint}/requisite-assignment-algorithm");
        if (response.Code == HttpStatusCode.OK &&
            !string.IsNullOrEmpty(response.Content) &&
            Enum.TryParse<RequisiteAssignmentAlgorithm>(response.Content.Trim('"'), out var algorithmEnum))
        {
            return algorithmEnum;
        }

        return RequisiteAssignmentAlgorithm.Priority;
    }

    public async Task<Response> SetRequisiteAssignmentAlgorithm(int algorithm)
    {
        return await PutRequest($"{apiEndpoint}/requisite-assignment-algorithm?algorithm={algorithm}");
    }

    public async Task<decimal> GetCurrentUsdtExchangeRate()
    {
        var response = await GetRequest($"{apiEndpoint}/usdt-exchange-rate");
        if (response.Code == HttpStatusCode.OK && 
            !string.IsNullOrEmpty(response.Content) &&
            decimal.TryParse(response.Content, out var rate))
        {
            return rate;
        }

        return -1;
    }

    public async Task<Response> SetCurrentUsdtExchangeRate(decimal rate)
    {
        return await PutRequest($"{apiEndpoint}/usdt-exchange-rate?rate={rate}");
    }
}