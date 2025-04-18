﻿using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class AdminService(
    IHttpClientFactory factory,
    ILogger<AdminService> logger,
    JsonSerializerOptions jsonOptions,
    AuthenticationStateProvider authStateProvider) : ServiceBase(factory, logger, jsonOptions), IAdminService
{
    private const string ApiEndpoint = "Admin";

    public async Task<List<UserDto>> GetAllUsers()
    {
        var response = await GetRequest($"{ApiEndpoint}/GetAllUsers");
        if (response.Code == System.Net.HttpStatusCode.OK)
        {
            return JsonSerializer.Deserialize<List<UserDto>>(response.Content ?? string.Empty, JsonOptions) ?? [];
        }
        return [];
    }

    public async Task<UserDto?> CreateUser(CreateUserDto dto)
    {
        return await PostRequest<UserDto>($"{ApiEndpoint}/CreateUser", dto);
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

    public async Task<bool> DeleteUser(Guid id)
    {
        var response = await DeleteRequest($"{ApiEndpoint}/DeleteUser/{id}");
        return response.Code == System.Net.HttpStatusCode.OK;
    }

    public async Task<UserDto?> UpdateUser(UpdateUserDto dto)
    {
        var response = await PutRequest($"{ApiEndpoint}/UpdateUser", dto);
        if (response.Code == System.Net.HttpStatusCode.OK)
        {
            return JsonSerializer.Deserialize<UserDto>(response.Content ?? string.Empty, JsonOptions);
        }
        return null;
    }

    public async Task<Dictionary<Guid, string>> GetUsersRoles(List<Guid> userIds)
    {
        try
        {
            var response = await GetRequest($"{ApiEndpoint}/GetUsersRoles?userIds={string.Join(",", userIds)}");
            if (response.Code == System.Net.HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<Dictionary<Guid, string>>(response.Content ?? string.Empty, JsonOptions) ?? [];
            }
            return [];
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении ролей пользователей");
            throw;
        }
    }
}