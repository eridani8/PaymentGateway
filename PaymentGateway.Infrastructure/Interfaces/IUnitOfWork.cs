using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface IUnitOfWork
{
    AppDbContext Context { get; }
    IRequisiteRepository RequisiteRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    IChatRepository ChatRepository { get; }
    IDeviceRepository DeviceRepository { get; }
    Task Commit(CancellationToken cancellationToken = default);
}