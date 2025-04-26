using Microsoft.Extensions.Logging;

namespace PaymentGateway.PhoneApp.Services;

public class BackgroundServiceManager(
    AvailabilityHost availabilityHost,
    ILogger<BackgroundServiceManager> logger)
{
    private readonly CancellationTokenSource _cts = new();
    
    public async Task StartAllServicesAsync()
    {
        try
        {
            await availabilityHost.StartAsync(_cts.Token);
            logger.LogInformation("Фоновые сервисы запущены");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при запуске фоновых сервисов");
            throw;
        }
    }
    
    public async Task StopAllServicesAsync()
    {
        try
        {
            await availabilityHost.StopAsync(CancellationToken.None);
            await _cts.CancelAsync();
            logger.LogInformation("Фоновые сервисы успешно остановлены");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при остановке фоновых сервисов");
        }
    }
}