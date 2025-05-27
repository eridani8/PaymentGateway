namespace PaymentGateway.Application.Results;

public static class PaymentErrors
{
    public static Error PaymentNotFound => 
        new(ErrorCode.PaymentNotFound, "Платеж не найден");
        
    public static Error DuplicatePayment => 
        new(ErrorCode.DuplicatePayment, "Платеж с таким внешним идентификатором уже существует");
    
    public static Error PaymentAlreadyConfirmed => 
        new(ErrorCode.PaymentAlreadyConfirmed, "Платеж уже подтвержден");
        
    public static Error RequisiteNotAttached => 
        new(ErrorCode.RequisiteNotAttached, "К платежу не привязан реквизит");
    
    public static Error InsufficientPermissionsForPayment => 
        new(ErrorCode.InsufficientPermissionsForPayment, "Недостаточно прав для подтверждения платежа");

    public static Error NotEnoughFunds =>
        new Error(ErrorCode.NotEnoughFunds, "Недостаточно средств");
}