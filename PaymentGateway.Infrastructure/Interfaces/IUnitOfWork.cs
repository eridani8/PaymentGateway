using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Interfaces.Repositories;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Interfaces;

public interface IUnitOfWork
{
    AppDbContext Context { get; }
    IRequisiteRepository RequisiteRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    IChatMessageRepository ChatMessageRepository { get; }
    Task Commit();
}