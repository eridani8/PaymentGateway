namespace PaymentGateway.Core.Interfaces;

public interface ITransactionManager
{
    Task Commit();
}