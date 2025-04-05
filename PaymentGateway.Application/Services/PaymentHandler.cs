using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class PaymentHandler(IServiceProvider serviceProvider, ILogger<PaymentHandler> logger) : IHostedService
{
    private Task _worker = null!;
    private CancellationTokenSource _cts = null!;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _worker = Worker();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _cts.CancelAsync();
            await Task.WhenAny(_worker, Task.Delay(Timeout.Infinite, cancellationToken));
        }
        finally
        {
            logger.LogInformation("Сервис остановлен");
            _cts.Dispose();
            _worker.Dispose();
        }
    }

    private async Task Worker()
    {
        await Task.Delay(3000, _cts.Token);
        logger.LogInformation("Сервис запущен");
        
        while (!_cts.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var requisiteService = scope.ServiceProvider.GetRequiredService<IRequisiteService>();
            
            var expiredPaymentHandler = scope.ServiceProvider.GetRequiredService<IExpiredPaymentHandler>();
            await expiredPaymentHandler.HandleExpiredPayments(unit);

            var unprocessedPaymentHandler = scope.ServiceProvider.GetRequiredService<IUnprocessedPaymentHandler>();
            await unprocessedPaymentHandler.HandleUnprocessedPayments(unit, requisiteService);

            await Task.Delay(1000, _cts.Token);
        }
    }
}