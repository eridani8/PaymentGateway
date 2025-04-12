using PaymentGateway.Web.Services;

namespace PaymentGateway.Web.Interfaces;

public interface IServiceBase
{
    Task<Response> CreateRequest(string url, object model);
    Task<T?> CreateRequest<T>(string url);
}