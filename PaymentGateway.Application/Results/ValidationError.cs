namespace PaymentGateway.Application.Results;

public record ValidationError(IEnumerable<string> Details) : Error(ErrorCode.Validation, string.Join(", ", Details)); 