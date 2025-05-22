namespace PaymentGateway.Application.Results;

public static class DeviceErrors
{
    public static Error BindingError => 
        new(ErrorCode.DeviceBindingError, "Ошибка привязки устройства");
}