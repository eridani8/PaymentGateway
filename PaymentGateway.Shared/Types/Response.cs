using System.Net;

namespace PaymentGateway.Shared.Types;

public class Response
{
    public HttpStatusCode Code { get; init; }
    public string? Content { get; init; }
}

public class Response<T> : Response
{
    public T? Data { get; set; }
}