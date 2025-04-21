using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Services;

public class GatewayHost(IServiceProvider serviceProvider, ILogger<GatewayHost> logger) : IHostedService
{
    private readonly TimeSpan _startDelay = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _gatewayProcessDelay = TimeSpan.FromSeconds(3);
    private readonly TimeSpan _fundsResetDelay = TimeSpan.FromHours(12);
    
    private Task _paymentProcessing = null!;
    private Task _fundsResetProcessing = null!;
    private CancellationTokenSource _cts = null!;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _paymentProcessing = GatewayProcess();
        _fundsResetProcessing = UserFundsResetProcess();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Сервис останавливается");
            
            await _cts.CancelAsync();
            
            try
            {
                if (_paymentProcessing != null) await _paymentProcessing;
            }
            catch (TaskCanceledException) { }
            
            try
            {
                if (_fundsResetProcessing != null)
                    await _fundsResetProcessing;
            }
            catch (TaskCanceledException) { }
            
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
            _paymentProcessing?.Dispose();
            _fundsResetProcessing?.Dispose();
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
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке платежного цикла");
            }
            finally
            {
                try
                {
                    await Task.Delay(_gatewayProcessDelay, _cts.Token);
                }
                catch (OperationCanceledException) { }
            }
        }
    }
    
    private async Task UserFundsResetProcess()
    {
        await Task.Delay(_startDelay, _cts.Token);
        logger.LogInformation("Цикл сброса средств пользователей запущен");
        
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IGatewayHandler>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();

                await handler.HandleUserFundsReset(userManager);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке цикла сброса средств пользователей");
            }
            finally
            {
                try
                {
                    await Task.Delay(_fundsResetDelay, _cts.Token);
                }
                catch (OperationCanceledException) { }
            }
        }
    }
}