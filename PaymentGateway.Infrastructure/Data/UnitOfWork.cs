using Microsoft.Extensions.Logging;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Interfaces;
using PaymentGateway.Infrastructure.Repositories;

namespace PaymentGateway.Infrastructure.Data;

public sealed class UnitOfWork(
    AppDbContext context,
    IRequisiteRepository requisiteRepository,
    IPaymentRepository paymentRepository,
    ITransactionRepository transactionRepository,
    IChatRepository chatRepository,
    IDeviceRepository deviceRepository,
    ISettingsRepository settingsRepository,
    ILogger<UnitOfWork> logger) : IUnitOfWork, IDisposable
{
    public AppDbContext Context { get; } = context;
    
    private bool _disposed;
    
    public IRequisiteRepository RequisiteRepository { get; } = requisiteRepository;
    public IPaymentRepository PaymentRepository { get; } = paymentRepository;
    public ITransactionRepository TransactionRepository { get; } = transactionRepository;
    public IChatRepository ChatRepository { get; } = chatRepository;
    public IDeviceRepository DeviceRepository { get; } = deviceRepository;
    public ISettingsRepository SettingsRepository { get; } = settingsRepository;

    public async Task Commit(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }

        try
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при сохранении изменений");
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