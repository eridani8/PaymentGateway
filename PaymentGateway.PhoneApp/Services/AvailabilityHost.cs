using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.Services;

public class AvailabilityHost(IAvailabilityChecker checker, ILogger<AvailabilityHost> logger) : IHostedService
{
    private readonly TimeSpan _startDelay = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _processDelay = TimeSpan.FromSeconds(10);
    
    private Task _task = null!;
    private CancellationTokenSource _cts = null!;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _task = Worker();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _cts.CancelAsync();

            try
            {
                await _task;
            }
            catch (TaskCanceledException) { }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при остановке сервиса проверки доступности");
        }
        finally
        {
            _cts.Dispose();
            _task.Dispose();
        }
    }

    private async Task Worker()
    {
        await Task.Delay(_startDelay, _cts.Token);
        
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                var result = await checker.CheckAvailable();
                logger.LogDebug("Проверка доступности: {state}", result);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка в процессе проверки доступности");
            }
            finally
            {
                try
                {
                    await Task.Delay(_processDelay, _cts.Token);
                }
                catch (OperationCanceledException) { }
            }
        }
    }
}