namespace PaymentGateway.Application;

public static class TimeOnlyExtensions
{
    private const string UserTimeZoneId = "Europe/Moscow";
    
    public static TimeOnly LocalToUtcTimeOnly(this TimeOnly localTime)
    {
        if (localTime == TimeOnly.MinValue) return TimeOnly.MinValue;
        
        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(UserTimeZoneId);

        var utcNow = DateTime.UtcNow;
        var userLocalNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, userTimeZone);
        var userLocalDate = userLocalNow.Date;

        var localDateTime = userLocalDate + localTime.ToTimeSpan();
        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, userTimeZone);

        return TimeOnly.FromDateTime(utcDateTime);
    }
    
    public static TimeOnly UtcToLocalTimeOnly(this TimeOnly utcTime)
    {
        if (utcTime == TimeOnly.MinValue) return TimeOnly.MinValue;
        
        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(UserTimeZoneId);

        var utcNow = DateTime.UtcNow;
        var utcDate = utcNow.Date;

        var utcDateTime = utcDate + utcTime.ToTimeSpan();
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, userTimeZone);

        return TimeOnly.FromDateTime(localDateTime);
    }
}