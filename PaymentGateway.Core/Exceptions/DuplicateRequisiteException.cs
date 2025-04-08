namespace PaymentGateway.Core.Exceptions;

public class DuplicateRequisiteException : Exception
{
    public DuplicateRequisiteException()
    {
    }

    public DuplicateRequisiteException(string message) : base(message)
    {
    }

    public DuplicateRequisiteException(string message, Exception innerException) : base(message, innerException)
    {
    }
}