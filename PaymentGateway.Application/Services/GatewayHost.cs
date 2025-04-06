using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Application.Services;

public class GatewayHost(IServiceProvider serviceProvider, ILogger<GatewayHost> logger, ICache cache) : IHostedService
{
    private readonly TimeSpan _startDelay = TimeSpan.FromSeconds(3);
    private readonly TimeSpan _paymentProcessInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _requisiteProcessInterval = TimeSpan.FromMinutes(1);
    
    private Task _paymentProcessing = null!;
    private Task _requisitesCheck = null!;
    private CancellationTokenSource _cts = null!;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var requisites = await unit.RequisiteRepository.GetAll().AsNoTracking().ToListAsync(cancellationToken);

        // TODO
        
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _paymentProcessing = PaymentsProcess();
        _requisitesCheck = RequisitesCheck();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _cts.CancelAsync();
            await Task.WhenAny(_paymentProcessing, _requisitesCheck, Task.Delay(Timeout.Infinite, cancellationToken));
            
            using var scope = serviceProvider.CreateScope();
            var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unit.Commit();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при остановке сервиса");
        }
        finally
        {
            _cts.Dispose();
            _paymentProcessing.Dispose();
            _requisitesCheck.Dispose();
            logger.LogInformation("Сервис остановлен");
        }
    }

    private async Task PaymentsProcess()
    {
        await Task.Delay(_startDelay, _cts.Token);
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var paymentHandler = scope.ServiceProvider.GetRequiredService<IPaymentHandler>();

                await paymentHandler.HandleUnprocessedPayments(unit);
                await paymentHandler.HandleExpiredPayments(unit);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке платежей");
            }
            finally
            {
                await Task.Delay(_paymentProcessInterval, _cts.Token);
            }
        }
    }

    private async Task RequisitesCheck()
    {
        await Task.Delay(_startDelay, _cts.Token);
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var requisites = await unit.RequisiteRepository.GetAll().ToListAsync();

                // TODO
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке реквизитов");
            }
            finally
            {
                await Task.Delay(_requisiteProcessInterval, _cts.Token);
            }
        }
    }
}