namespace PaymentGateway.Core.Exceptions;

public class DeleteUserException : Exception
{
    public DeleteUserException()
    {
    }

    public DeleteUserException(string message) : base(message)
    {
    }

    public DeleteUserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}