using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class ServiceBase(
    IHttpClientFactory factory, 
    ILogger<ServiceBase> logger) : IServiceBase
{
    public async Task<Response> CreateRequest(string url, object model)
    {
        HttpResponseMessage? response = null;
        string? content = null;
        try
        {
            using var client = factory.CreateClient("API");
            response = await client.PostAsJsonAsync(url, model);
            content = await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
        return new Response()
        {
            Code = response?.StatusCode ?? HttpStatusCode.InternalServerError,
            Content = content
        };
    }
    
    public async Task<T?> CreateRequest<T>(string url, object model)
    {
        T? response = default;
        try
        {
            using var client = factory.CreateClient("API");
            var httpResponse = await client.PostAsJsonAsync(url, model);
            
            if (httpResponse.IsSuccessStatusCode)
            {
                response = await httpResponse.Content.ReadFromJsonAsync<T>();
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
        return response;
    }
    
    public async Task<T?> CreateRequest<T>(string url)
    {
        T? response = default;
        try
        {
            using var client = factory.CreateClient("API");
            var httpResponse = await client.GetAsync(url);
            
            if (httpResponse.IsSuccessStatusCode)
            {
                response = await httpResponse.Content.ReadFromJsonAsync<T>();
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
        return response;
    }
}