namespace PaymentGateway.Core.Exceptions;

public class RequisiteNotFound : Exception
{
    public RequisiteNotFound()
    {
    }

    public RequisiteNotFound(string message) : base(message)
    {
    }

    public RequisiteNotFound(string message, Exception innerException) : base(message, innerException)
    {
    }
}