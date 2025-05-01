namespace PaymentGateway.Application.Results;

public static class RequisiteErrors
{
    public static Error RequisiteNotFound => 
        new(ErrorCode.RequisiteNotFound, "Реквизит не найден");
        
    public static Error DuplicateRequisite => 
        new(ErrorCode.DuplicateRequisite, "Реквизит с такими платежными данными уже существует");
    
    public static Error RequisiteLimitExceeded(int maxCount) => 
        new(ErrorCode.RequisiteLimitExceeded, $"Достигнут лимит реквизитов. Максимум: {maxCount}");
}