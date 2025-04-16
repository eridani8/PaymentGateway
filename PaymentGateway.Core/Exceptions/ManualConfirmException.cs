namespace PaymentGateway.Core.Exceptions;

public class ManualConfirmException : Exception
{
    public ManualConfirmException()
    {
    }

    public ManualConfirmException(string message) : base(message)
    {
    }

    public ManualConfirmException(string message, Exception innerException) : base(message, innerException)
    {
    }
}