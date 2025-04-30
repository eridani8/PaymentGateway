using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class AdminService(
    IHttpClientFactory factory,
    ILogger<AdminService> logger,
    JsonSerializerOptions jsonOptions,
    AuthenticationStateProvider authStateProvider) : ServiceBase(factory, logger, jsonOptions), IAdminService
{
    private const string apiEndpoint = "Admin";

    public async Task<List<UserDto>> GetAllUsers()
    {
        var response = await GetRequest($"{apiEndpoint}/GetAllUsers");
        if (response.Code == HttpStatusCode.OK)
        {
            return JsonSerializer.Deserialize<List<UserDto>>(response.Content ?? string.Empty, JsonOptions) ?? [];
        }

        return [];
    }

    public async Task<Guid?> CreateUser(CreateUserDto dto)
    {
        return await PostRequest<Guid>($"{apiEndpoint}/CreateUser", dto);
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

        var allPayments = await GetAllUsers();
        return allPayments.FirstOrDefault(u => u.Id == id);
    }

    public async Task<Response> DeleteUser(Guid id)
    {
        return await DeleteRequest($"{apiEndpoint}/DeleteUser/{id}");
    }

    public async Task<Response> UpdateUser(UpdateUserDto dto)
    {
        return await PutRequest($"{apiEndpoint}/UpdateUser", dto);
    }

    public async Task<Dictionary<Guid, string>> GetUsersRoles(List<Guid> userIds)
    {
        try
        {
            var response = await GetRequest($"{apiEndpoint}/GetUsersRoles?userIds={string.Join(",", userIds)}");
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

    public async Task<bool> ResetTwoFactor(Guid userId)
    {
        var response = await PutRequest($"{apiEndpoint}/ResetTwoFactor/{userId}");
        return response.Code == HttpStatusCode.OK;
    }

    public async Task<RequisiteAssignmentAlgorithm> GetCurrentRequisiteAssignmentAlgorithm()
    {
        var response = await GetRequest($"{apiEndpoint}/GetCurrentRequisiteAssignmentAlgorithm");
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
        return await PutRequest($"{apiEndpoint}/SetRequisiteAssignmentAlgorithm", algorithm);
    }
}