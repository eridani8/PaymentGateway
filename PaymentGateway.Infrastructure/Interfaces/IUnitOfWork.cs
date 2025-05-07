using PaymentGateway.Core.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface IUnitOfWork
{
    AppDbContext Context { get; }
    IRequisiteRepository RequisiteRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    IChatRepository ChatRepository { get; }
    Task Commit(CancellationToken cancellationToken = default);
}