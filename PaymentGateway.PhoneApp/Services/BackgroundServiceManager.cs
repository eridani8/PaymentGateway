using Microsoft.Extensions.Logging;

namespace PaymentGateway.PhoneApp.Services;

public class BackgroundServiceManager(
    AvailabilityHost availabilityHost,
    ILogger<BackgroundServiceManager> logger)
{
    private readonly CancellationTokenSource _cts = new();
    
    public async Task StartAllServicesAsync()
    {
        logger.LogInformation("Запуск фоновых сервисов");
        
        try
        {
            await availabilityHost.StartAsync(_cts.Token);
            logger.LogInformation("Фоновые сервисы успешно запущены");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при запуске фоновых сервисов");
            throw;
        }
    }
    
    public async Task StopAllServicesAsync()
    {
        logger.LogInformation("Остановка фоновых сервисов");
        
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