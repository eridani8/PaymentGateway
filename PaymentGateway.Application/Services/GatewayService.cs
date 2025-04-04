﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;
using Serilog;

namespace PaymentGateway.Application.Services;

public class GatewayService(IServiceProvider serviceProvider, ILogger<GatewayService> logger) : IHostedService
{
    private Task? _worker;
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
            if (_worker != null)
            {
                await Task.WhenAny(_worker, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }
        finally
        {
            logger.LogInformation("Сервис остановлен");
            _cts.Dispose();
            _worker?.Dispose();
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

            var expiredPayments = await unit.PaymentRepository.GetExpiredPayments();
            if (expiredPayments.Count > 0)
            {
                foreach (var expiredPayment in expiredPayments)
                {
                    logger.LogInformation("Платеж {payment} просрочен и был удален", expiredPayment.Id);
                }
                
                await unit.PaymentRepository.DeletePayments(expiredPayments);
                await unit.Commit();
            }
            
            var unprocessedPayments = await unit.PaymentRepository.GetUnprocessedPayments();
            
            if (unprocessedPayments.Count == 0)
            {
                await Task.Delay(1000, _cts.Token); 
                continue;
            }
            
            var freeRequisites = await unit.RequisiteRepository.GetFreeRequisites();

            foreach (var payment in unprocessedPayments)
            {
                if (freeRequisites.Count == 0)
                {
                    logger.LogWarning("Нет свободных реквизитов для обработки платежей");
                    break;
                }

                var requisite = freeRequisites.First();
                freeRequisites.Remove(requisite);
                
                payment.RequisiteId = requisite.Id;
                payment.Status = PaymentStatus.Pending;
                
                requisite.CurrentPaymentId = payment.Id;
                requisite.LastOperationTime = DateTime.UtcNow;

                logger.LogInformation("Платеж {payment} назначен реквизиту {requisite}", payment.Id, requisite.Id);
            }

            await unit.Commit();
            
            await Task.Delay(1000, _cts.Token);
        }
    }
}