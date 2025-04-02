namespace PaymentGateway.Core.Interfaces;

public interface IUnitOfWork
{
    IRequisiteRepository RequisiteRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    Task Commit();
}