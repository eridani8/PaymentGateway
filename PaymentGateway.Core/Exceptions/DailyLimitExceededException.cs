namespace PaymentGateway.Core.Exceptions;

public class DailyLimitExceededException : Exception
{
    public DailyLimitExceededException()
    {
    }

    public DailyLimitExceededException(string message) : base(message)
    {
    }

    public DailyLimitExceededException(string message, Exception innerException) : base(message, innerException)
    {
    }
} 