namespace PaymentGateway.Application;

public static class TimeOnlyExtensions
{
    private const string UserTimeZoneId = "Europe/Moscow";
    
    public static TimeOnly LocalToUtcTimeOnly(this TimeOnly localTime)
    {
        if (localTime == TimeOnly.MinValue) return TimeOnly.MinValue;
        
        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(UserTimeZoneId);

        var fixedDate = new DateTime(2000, 1, 1);
        
        var localDateTime = fixedDate + localTime.ToTimeSpan();
        localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        
        // Преобразуем в UTC
        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, userTimeZone);

        return TimeOnly.FromDateTime(utcDateTime);
    }
    
    public static TimeOnly UtcToLocalTimeOnly(this TimeOnly utcTime)
    {
        if (utcTime == TimeOnly.MinValue) return TimeOnly.MinValue;
        
        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(UserTimeZoneId);

        var fixedDate = new DateTime(2000, 1, 1);
        
        var utcDateTime = fixedDate + utcTime.ToTimeSpan();
        utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, userTimeZone);

        return TimeOnly.FromDateTime(localDateTime);
    }
}