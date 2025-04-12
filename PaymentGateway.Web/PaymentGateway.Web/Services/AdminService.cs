﻿using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class AdminService(IHttpClientFactory factory, ILogger<AdminService> logger) : ServiceBase(factory, logger), IAdminService
{
    private const string ApiEndpoint = "users";
    
    public async Task<List<UserDto>> GetUsers()
    {
        var users = await CreateRequest<List<UserDto>>($"{ApiEndpoint}/GetAllUsers");
        return users ?? [];
    }
}