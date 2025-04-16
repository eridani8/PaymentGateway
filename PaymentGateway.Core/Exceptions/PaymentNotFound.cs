namespace PaymentGateway.Core.Exceptions;

public class PaymentNotFound : Exception
{
    public PaymentNotFound()
    {
    }

    public PaymentNotFound(string message) : base(message)
    {
    }

    public PaymentNotFound(string message, Exception innerException) : base(message, innerException)
    {
    }
}