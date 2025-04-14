using System.Text.Json;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class AdminService(
    IHttpClientFactory factory,
    ILogger<AdminService> logger) : ServiceBase(factory, logger), IAdminService
{
    private const string ApiEndpoint = "Users";

    public async Task<List<UserDto>> GetUsers()
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
    
    public async Task<UserDto?> DeleteUser(Guid id)
    {
        return await DeleteRequest<UserDto?>($"{ApiEndpoint}/DeleteUser/{id}");
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
}