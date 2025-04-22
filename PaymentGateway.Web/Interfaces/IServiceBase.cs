using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IServiceBase
{
    Task<T?> PostRequest<T>(string url, object model);
    Task<Response> PostRequest(string url, object model);
    Task<Response> PostRequest(string url);
    Task<Response> GetRequest(string url);
    Task<Response> PutRequest(string url, object model);
    Task<T?> PutRequest<T>(string url, object model);
    Task<T?> DeleteRequest<T>(string url);
}