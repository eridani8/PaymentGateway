using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;
using Serilog;

namespace PaymentGateway.Application.Services;

public class GatewayService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var requisiteService = scope.ServiceProvider.GetRequiredService<IRequisiteService>();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

            var expiredPayments = await paymentService.GetExpiredPayments();
            if (expiredPayments.Count > 0)
            {
                foreach (var expiredPayment in expiredPayments)
                {
                    Log.ForContext<GatewayService>().Information("Платеж {payment} просрочен и был удален", expiredPayment.Id);
                }
                
                await paymentService.DeletePayments(expiredPayments);
                await unit.Commit();
            }
            
            var unprocessedPayments = await paymentService.GetUnprocessedPayments();
            
            if (unprocessedPayments.Count == 0)
            {
                await Task.Delay(1000, stoppingToken); 
                continue;
            }
            
            var freeRequisites = await requisiteService.GetFreeRequisites();

            foreach (var payment in unprocessedPayments)
            {
                if (freeRequisites.Count == 0)
                {
                    Log.ForContext<GatewayService>().Warning("Нет свободных реквизитов для обработки платежей");
                    break;
                }

                var requisite = freeRequisites.First();
                freeRequisites.Remove(requisite);
                
                payment.RequisiteId = requisite.Id;
                payment.Status = PaymentStatus.Pending;
                
                requisite.CurrentPaymentId = payment.Id;
                requisite.LastOperationTime = DateTime.UtcNow;

                Log.ForContext<GatewayService>().Information("Платеж {payment} назначен реквизиту {requisite}", payment.Id, requisite.Id);
            }

            await unit.Commit();
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}