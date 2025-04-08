﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class GatewayHost(IServiceProvider serviceProvider, ILogger<GatewayHost> logger, ICache cache) : IHostedService
{
    private readonly TimeSpan _startDelay = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _gatewayProcessInterval = TimeSpan.FromSeconds(1);
    
    private Task _paymentProcessing = null!;
    private CancellationTokenSource _cts = null!;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _paymentProcessing = GatewayProcess();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _cts.CancelAsync();
            await Task.WhenAny(_paymentProcessing, Task.Delay(Timeout.Infinite, cancellationToken));
            
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
            logger.LogInformation("Сервис остановлен");
        }
    }

    private async Task GatewayProcess()
    {
        await Task.Delay(_startDelay, _cts.Token);
        logger.LogInformation("Сервис запущен");
        
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var handler = scope.ServiceProvider.GetRequiredService<IGatewayHandler>();

                await handler.HandleRequisites(unit);
                
                await handler.HandleUnprocessedPayments(unit);
                
                await handler.HandleExpiredPayments(unit);

                await unit.Commit();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке платежей");
            }
            finally
            {
                await Task.Delay(_gatewayProcessInterval, _cts.Token);
            }
        }
    }
}