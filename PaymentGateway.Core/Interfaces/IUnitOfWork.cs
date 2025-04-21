using PaymentGateway.Core.Interfaces.Repositories;

namespace PaymentGateway.Core.Interfaces;

public interface IUnitOfWork
{
    IRequisiteRepository RequisiteRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    IChatMessageRepository ChatMessageRepository { get; }
    Task Commit();
}