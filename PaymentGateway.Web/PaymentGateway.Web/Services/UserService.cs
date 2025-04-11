using System.Net;
using System.Net.Http.Json;
using PaymentGateway.Shared.Models;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class UserService(IHttpClientFactory factory) : IUserService
{
    public async Task<(HttpStatusCode, string?)> Login(LoginModel model)
    {
        HttpResponseMessage? response = null;
        string? content = null;
        try
        {
            using var client = factory.CreateClient("API");
            response = await client.PostAsJsonAsync("auth/login", model);
            content = await response.Content.ReadAsStringAsync();
        }
        catch
        {
            // ignore
        }
        return (response?.StatusCode ?? HttpStatusCode.InternalServerError, content);
    }
    
    public async Task<(HttpStatusCode, string?)> ChangePasswordAsync(ChangePasswordModel model)
    {
        HttpResponseMessage? response = null;
        string? content = null;
        try
        {
            using var client = factory.CreateClient("API");
            response = await client.PostAsJsonAsync("api/user/change-password", model);
            content = await response.Content.ReadAsStringAsync();
        }
        catch
        {
            // ignore
        }
        return (response?.StatusCode ?? HttpStatusCode.InternalServerError, content);
    }
}