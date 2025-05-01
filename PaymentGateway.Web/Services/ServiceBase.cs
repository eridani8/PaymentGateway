using System.Net;
using System.Text;
using System.Text.Json;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class ServiceBase(IHttpClientFactory factory, ILogger<ServiceBase> logger, JsonSerializerOptions jsonOptions) : IServiceBase
{
    protected JsonSerializerOptions JsonOptions => jsonOptions;
    
    public async Task<Response> PostRequest(string url, object model)
    {
        return await SendRequest(HttpMethod.Post, url, model);
    }
    
    public async Task<Response> PostRequest(string url)
    {
        return await SendRequest(HttpMethod.Post, url);
    }

    public async Task<T?> PostRequest<T>(string url, object model)
    {
        var response = await SendRequest(HttpMethod.Post, url, model);
        return response.Code is HttpStatusCode.OK or HttpStatusCode.Created 
            ? JsonSerializer.Deserialize<T>(response.Content?.Trim('"') ?? string.Empty, JsonOptions)
            : default;
    }

    public async Task<Response> GetRequest(string url)
    {
        return await SendRequest(HttpMethod.Get, url);
    }

    public async Task<Response> PutRequest(string url, object model)
    {
        return await SendRequest(HttpMethod.Put, url, model);
    }
    
    public async Task<Response> PutRequest(string url)
    {
        return await SendRequest(HttpMethod.Put, url);
    }

    public async Task<T?> PutRequest<T>(string url, object model)
    {
        var response = await SendRequest(HttpMethod.Put, url, model);
        return response.Code is HttpStatusCode.OK or HttpStatusCode.Created
            ? JsonSerializer.Deserialize<T>(response.Content?.Trim('"') ?? string.Empty, JsonOptions)
            : default;
    }

    public async Task<Response> DeleteRequest(string url)
    {
        return await SendRequest(HttpMethod.Delete, url);
    }

    public async Task<T?> DeleteRequest<T>(string url)
    {
        var response = await SendRequest(HttpMethod.Delete, url);
        return response.Code is HttpStatusCode.OK 
            ? JsonSerializer.Deserialize<T>(response.Content?.Trim('"') ?? string.Empty, JsonOptions)
            : default;
    }

    private async Task<Response> SendRequest(HttpMethod method, string url, object? model = null, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage? httpResponse = null;
        string? content = null;
        try
        {
            using var client = factory.CreateClient("API");
            var request = new HttpRequestMessage(method, url);

            if (model != null && (method != HttpMethod.Get && method != HttpMethod.Head && method != HttpMethod.Options))
            {
                var json = JsonSerializer.Serialize(model, JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            httpResponse = await client.SendAsync(request, cancellationToken);
            
            if (method != HttpMethod.Head)
            {
                content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("Request to {Url} failed with status {StatusCode}: {Content}", 
                    url, httpResponse.StatusCode, content);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Request to {Url} was cancelled", url);
            return new Response
            {
                Code = HttpStatusCode.RequestTimeout,
                Content = "Request cancelled"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during {Method} request to {Url}: {Message}", method, url, ex.Message);
        }

        return new Response
        {
            Code = httpResponse?.StatusCode ?? HttpStatusCode.InternalServerError,
            Content = content?.Trim('"')
        };
    }
}