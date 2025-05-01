namespace PaymentGateway.Application.Results;

public record Error
{
    public ErrorCode Code { get; }
    public string Message { get; }

    protected Error(ErrorCode code, string message)
    {
        Code = code;
        Message = message;
    }

    public static Error None => new(ErrorCode.None, string.Empty);
    public static Error NullValue => new(ErrorCode.NullValue, "Значение не может быть null");

    public static Error UserNotFound => 
        new(ErrorCode.UserNotFound, "Пользователь не найден");
    
    public static Error UserAlreadyExists => 
        new(ErrorCode.UserAlreadyExists, "Пользователь с таким именем уже существует");
    
    public static Error UserCreationFailed(string details) => 
        new(ErrorCode.UserCreationFailed, $"Ошибка при создании пользователя. {details}".TrimEnd());
    
    public static Error UserUpdateFailed(string details) => 
        new(ErrorCode.UserUpdateFailed, $"Ошибка при обновлении пользователя. {details}".TrimEnd());
    
    public static Error OperationFailed(string operation, string details) => 
        new(ErrorCode.OperationFailed, $"Операция {operation} не выполнена. {details}".TrimEnd());
    
    public static Error OperationFailed(string details) => 
        new(ErrorCode.OperationFailed, details.TrimEnd());
    
    public static Error AccessDenied => 
        new(ErrorCode.AccessDenied, "Отказано в доступе");
    
    public static Error DeleteRootUserForbidden => 
        new(ErrorCode.DeleteRootUserForbidden, "Нельзя удалить root пользователя");
    
    public static Error SelfDeleteForbidden => 
        new(ErrorCode.SelfDeleteForbidden, "Нельзя удалить себя");
    
    public static Error ModifyRootUserForbidden => 
        new(ErrorCode.ModifyRootUserForbidden, "Нельзя изменить root пользователя");
        
    public static Error PaymentNotFound => 
        new(ErrorCode.PaymentNotFound, "Платеж не найден");
        
    public static Error DuplicatePayment => 
        new(ErrorCode.DuplicatePayment, "Платеж с таким внешним идентификатором уже существует");
    
    public static Error PaymentAlreadyConfirmed => 
        new(ErrorCode.PaymentAlreadyConfirmed, "Платеж уже подтвержден");
        
    public static Error RequisiteNotAttached => 
        new(ErrorCode.RequisiteNotAttached, "К платежу не привязан реквизит");
        
    public static Error InsufficientPermissions => 
        new(ErrorCode.InsufficientPermissions, "Недостаточно прав");
        
    public static Error InsufficientPermissionsForPayment => 
        new(ErrorCode.InsufficientPermissionsForPayment, "Недостаточно прав для подтверждения платежа");
        
    public static Error RequisiteNotFound => 
        new(ErrorCode.RequisiteNotFound, "Реквизит не найден");
        
    public static Error DuplicateRequisite => 
        new(ErrorCode.DuplicateRequisite, "Реквизит с такими платежными данными уже существует");
    
    public static Error RequisiteLimitExceeded(int maxCount) => 
        new(ErrorCode.RequisiteLimitExceeded, $"Достигнут лимит реквизитов. Максимум: {maxCount}");
    
    public static Error TransactionError(string details) => 
        new(ErrorCode.TransactionError, $"Ошибка при обработке транзакции. {details}".TrimEnd());
    
    public static Error WrongPaymentAmount(decimal actual, decimal expected) => 
        new(ErrorCode.WrongPaymentAmount, $"Сумма платежа {actual}, ожидалось {expected}");
} 