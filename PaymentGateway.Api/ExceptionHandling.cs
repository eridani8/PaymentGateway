using Serilog;

namespace PaymentGateway.Api;

public class ExceptionHandling(RequestDelegate next, ILogger<ExceptionHandling> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Произошло необработанное исключение");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new
        {
            error = "Ошибка на сервере",
            details = exception.Message
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}