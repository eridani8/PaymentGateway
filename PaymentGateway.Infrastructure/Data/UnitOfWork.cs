using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Infrastructure.Data;

public sealed class UnitOfWork(
    AppDbContext context,
    IRequisiteRepository requisiteRepository,
    IPaymentRepository paymentRepository,
    ITransactionRepository transactionRepository) : IUnitOfWork, IDisposable
{
    private bool _disposed;

    public IRequisiteRepository RequisiteRepository { get; } = requisiteRepository;
    public IPaymentRepository PaymentRepository { get; } = paymentRepository;
    public ITransactionRepository TransactionRepository { get; } = transactionRepository;

    public async Task Commit()
    {
        if (!_disposed)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }
    }

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