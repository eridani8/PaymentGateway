using Microsoft.Extensions.Logging;
using PaymentGateway.Core.Interfaces.Repositories;
using PaymentGateway.Infrastructure.Interfaces;

namespace PaymentGateway.Infrastructure.Data;

public sealed class UnitOfWork(
    AppDbContext context,
    IRequisiteRepository requisiteRepository,
    IPaymentRepository paymentRepository,
    ITransactionRepository transactionRepository,
    IChatRepository chatRepository,
    ILogger<UnitOfWork> logger) : IUnitOfWork, IDisposable
{
    public AppDbContext Context { get; } = context;
    
    private bool _disposed;
    
    public IRequisiteRepository RequisiteRepository { get; } = requisiteRepository;
    public IPaymentRepository PaymentRepository { get; } = paymentRepository;
    public ITransactionRepository TransactionRepository { get; } = transactionRepository;
    public IChatRepository ChatRepository { get; } = chatRepository;

    public async Task Commit()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }

        try
        {
            await Context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при сохранении изменений");
            throw;
        }
    }
    
    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            Context.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
    }
}