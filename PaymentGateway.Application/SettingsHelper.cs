namespace PaymentGateway.Application;

public static class SettingsHelper
{
    public static T GetValueOrDefault<T>(T? dtoValue, T defaultValue) where T : struct, IComparable<T>
    {
        return dtoValue.HasValue && dtoValue.Value.CompareTo(default) > 0 ? dtoValue.Value : defaultValue;
    }
}