namespace PaymentGateway.Application.Results;

public class ValidationError(List<string> details) : Error(ErrorCode.Validation, string.Join(", ", details))
{
} 