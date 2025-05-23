namespace PaymentGateway.Application.Results;

public static class DeviceErrors
{
    public static Error DeviceShouldBeOnline =>
        new(ErrorCode.DeviceShouldBeOnline, "Устройство должно быть онлайн и не привязано");

    public static Error DeviceShouldNotBeTied =>
        new(ErrorCode.DeviceShouldNotBeTied, "Устройство уже привязано к реквизиту");
}