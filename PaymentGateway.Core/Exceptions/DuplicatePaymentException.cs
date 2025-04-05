namespace PaymentGateway.Core.Exceptions;

public class DuplicatePaymentException : Exception
{
    public DuplicatePaymentException()
    {
    }

    public DuplicatePaymentException(string message) : base(message)
    {
    }

    public DuplicatePaymentException(string message, Exception innerException) : base(message, innerException)
    {
    }
}