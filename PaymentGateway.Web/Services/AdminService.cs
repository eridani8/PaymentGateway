using System.Text.Json;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class AdminService(
    IHttpClientFactory factory,
    ILogger<AdminService> logger,
    JsonSerializerOptions jsonOptions) : ServiceBase(factory, logger, jsonOptions), IAdminService
{
    private const string ApiEndpoint = "Users";

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

    public Task<UserDto?> GetUserById(Guid id)
    {
        throw new NotImplementedException();
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
}