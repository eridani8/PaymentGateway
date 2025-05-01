namespace PaymentGateway.Application.Results;

public record ValidationError : Error
{
    public ValidationError(IEnumerable<string> details) : base(ErrorCode.Validation, string.Join(", ", details))
    {
    }
}