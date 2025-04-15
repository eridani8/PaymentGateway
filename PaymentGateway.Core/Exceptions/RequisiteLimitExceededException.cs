namespace PaymentGateway.Core.Exceptions;

public class RequisiteLimitExceededException : Exception
{
    public RequisiteLimitExceededException()
    {
    }

    public RequisiteLimitExceededException(string message) : base(message)
    {
    }

    public RequisiteLimitExceededException(string message, Exception innerException) : base(message, innerException)
    {
    }
} 