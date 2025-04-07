using Microsoft.AspNetCore.Diagnostics;

namespace PaymentGateway.Api;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Необработанное исключение");

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        const string response = "Ошибка на стороне сервере";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}