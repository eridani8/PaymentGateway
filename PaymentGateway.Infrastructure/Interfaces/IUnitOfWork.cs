using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Repositories;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface IUnitOfWork
{
    AppDbContext Context { get; }
    IRequisiteRepository RequisiteRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    IChatRepository ChatRepository { get; }
    IDeviceRepository DeviceRepository { get; }
    ISettingsRepository SettingsRepository { get; }
    Task Commit(CancellationToken cancellationToken = default);
}