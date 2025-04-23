using Microsoft.Extensions.Logging;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Interfaces.Repositories;

namespace PaymentGateway.Infrastructure.Data;

public sealed class UnitOfWork(
    AppDbContext context,
    IRequisiteRepository requisiteRepository,
    IPaymentRepository paymentRepository,
    ITransactionRepository transactionRepository,
    IChatMessageRepository chatMessageRepository,
    ILogger<UnitOfWork> logger) : IUnitOfWork, IDisposable
{
    private bool _disposed;

    public IRequisiteRepository RequisiteRepository { get; } = requisiteRepository;
    public IPaymentRepository PaymentRepository { get; } = paymentRepository;
    public ITransactionRepository TransactionRepository { get; } = transactionRepository;
    public IChatMessageRepository ChatMessageRepository { get; } = chatMessageRepository;

    public async Task Commit()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }

        try
        {
            // var entries = context.ChangeTracker.Entries()
            //     .Where(e => e.State is EntityState.Modified or EntityState.Deleted)
            //     .ToList();

            await context.SaveChangesAsync();

            // foreach (var entry in entries)
            // {
            //     if (entry.Entity is not ICacheable cacheable) continue;
            //     try
            //     {
            //         switch (entry.State)
            //         {
            //             case EntityState.Modified:
            //                 UpdateEntityCache(entry.Entity);
            //                 break;
            //             case EntityState.Deleted:
            //                 InvalidateEntityCache(entry.Entity);
            //                 break;
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         logger.LogError(ex, "Ошибка при обновлении кеша для сущности {entityType} с ID {entityId}",
            //             entry.Entity.GetType().Name, cacheable.Id);
            //     }
            // }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при сохранении изменений");
            throw;
        }
    }

    // private void UpdateEntityCache(object entity)
    // {
    //     switch (entity)
    //     {
    //         case RequisiteEntity requisite:
    //             RequisiteRepository.UpdateCache(requisite);
    //             break;
    //         case PaymentEntity payment:
    //             PaymentRepository.UpdateCache(payment);
    //             break;
    //     }
    // }
    //
    // private void InvalidateEntityCache(object entity)
    // {
    //     switch (entity)
    //     {
    //         case RequisiteEntity requisite:
    //             RequisiteRepository.InvalidateCache(requisite);
    //             break;
    //         case PaymentEntity payment:
    //             PaymentRepository.InvalidateCache(payment);
    //             break;
    //     }
    // }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            context.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
    }
}