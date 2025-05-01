namespace PaymentGateway.Application.Results;

public record Error(ErrorCode Code, string Message)
{
    public static Error None => new(ErrorCode.None, string.Empty);
    public static Error NullValue => new(ErrorCode.NullValue, "Значение не может быть null");
    
    public static Error OperationFailed(string operation, string details) => 
        new(ErrorCode.OperationFailed, $"Операция {operation} не выполнена. {details}".TrimEnd());
    
    public static Error OperationFailed(string details) => 
        new(ErrorCode.OperationFailed, details.Trim());
} 