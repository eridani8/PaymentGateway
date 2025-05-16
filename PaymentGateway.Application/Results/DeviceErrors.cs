namespace PaymentGateway.Application.Results;

public static class DeviceErrors
{
    public static Error ModelIsEmpty => 
        new(ErrorCode.DeviceModelIsEmpty, "Модель устройства не может быть пустой");
}