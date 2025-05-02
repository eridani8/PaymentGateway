namespace PaymentGateway.Application.Results;

public static class UserErrors
{
    public static Error InsufficientPermissions => 
        new(ErrorCode.InsufficientPermissions, "Недостаточно прав");
    
    public static Error AccessDenied => 
        new(ErrorCode.AccessDenied, "Отказано в доступе");
    
    public static Error UserNotFound =>
        new(ErrorCode.UserNotFound, "Пользователь не найден");
    
    public static Error InappropriateData =>
        new(ErrorCode.InappropriateData, "Неправильный логин или пароль");
    
    public static Error InappropriateCode =>
        new(ErrorCode.InappropriateCode, "Неправильный код подтверждения");

    public static Error UserAlreadyExists =>
        new(ErrorCode.UserAlreadyExists, "Пользователь с таким именем уже существует");

    public static Error UserCreationFailed(string details) =>
        new(ErrorCode.UserCreationFailed, $"Ошибка при создании пользователя. {details}".TrimEnd());

    public static Error UserUpdateFailed(string details) =>
        new(ErrorCode.UserUpdateFailed, $"Ошибка при обновлении пользователя. {details}".TrimEnd());
    
    public static Error DeleteRootUserForbidden => 
        new(ErrorCode.DeleteRootUserForbidden, "Нельзя удалить root пользователя");
    
    public static Error SelfDeleteForbidden => 
        new(ErrorCode.SelfDeleteForbidden, "Нельзя удалить себя");
    
    public static Error ModifyRootUserForbidden => 
        new(ErrorCode.ModifyRootUserForbidden, "Нельзя изменить root пользователя");
}