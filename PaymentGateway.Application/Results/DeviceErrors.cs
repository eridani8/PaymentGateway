namespace PaymentGateway.Application.Results;

public static class DeviceErrors
{
    public static Error DeviceShouldBeOnline =>
        new(ErrorCode.DeviceShouldBeOnline, "Устройство должно быть онлайн и не привязано к другому реквизиту");
}