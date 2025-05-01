namespace PaymentGateway.Application.Results;

public static class TransactionErrors
{
    public static Error TransactionError(string details) => 
        new(ErrorCode.TransactionError, $"Ошибка при обработке транзакции. {details}".TrimEnd());
    
    public static Error WrongPaymentAmount(decimal actual, decimal expected) => 
        new(ErrorCode.WrongPaymentAmount, $"Сумма платежа {actual}, ожидалось {expected}");
}