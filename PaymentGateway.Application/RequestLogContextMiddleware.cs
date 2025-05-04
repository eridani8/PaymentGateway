using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace PaymentGateway.Application;

public class RequestLogContextMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
        {
            return next(context);
        }
    }
}