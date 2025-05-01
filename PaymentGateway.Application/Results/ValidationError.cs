namespace PaymentGateway.Application.Results;

public record ValidationError(List<string> Details) : Error(ErrorCode.Validation, string.Join(", ", Details))
{
} 