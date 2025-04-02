using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Repositories;

namespace PaymentGateway.Infrastructure.Data;

public sealed class TransactionManager(AppDbContext context) : ITransactionManager, IAsyncDisposable
{
    public async Task Commit()
    {
        await context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }
}