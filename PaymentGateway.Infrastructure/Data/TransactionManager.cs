using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Repositories;

namespace PaymentGateway.Infrastructure.Data;

public sealed class TransactionManager(AppDbContext context) : ITransactionManager, IDisposable
{
    private bool _disposed;
    
    public async Task Commit()
    {
        if (!_disposed)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            throw new ObjectDisposedException(nameof(TransactionManager));
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