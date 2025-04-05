namespace PaymentGateway.Core.Exceptions;

public class WrongPaymentAmount : Exception
{
    public WrongPaymentAmount()
    {
    }

    public WrongPaymentAmount(string message) : base(message)
    {
    }

    public WrongPaymentAmount(string message, Exception innerException) : base(message, innerException)
    {
    }
}