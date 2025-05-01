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
    
    public static Error AccessDenied => 
        new(ErrorCode.AccessDenied, "Отказано в доступе");
    
    public static Error DeleteRootUserForbidden => 
        new(ErrorCode.DeleteRootUserForbidden, "Нельзя удалить root пользователя");
    
    public static Error SelfDeleteForbidden => 
        new(ErrorCode.SelfDeleteForbidden, "Нельзя удалить себя");
    
    public static Error ModifyRootUserForbidden => 
        new(ErrorCode.ModifyRootUserForbidden, "Нельзя изменить root пользователя");
} 