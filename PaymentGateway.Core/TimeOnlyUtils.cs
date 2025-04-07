namespace PaymentGateway.Core;

public static class TimeOnlyUtils
{
    public static (TimeOnly WorkFromUtc, TimeOnly WorkToUtc) ToUtcTimeOnly(TimeOnly workFrom, TimeOnly workTo)
    {
        const string userTimeZoneId = "Europe/Moscow";
        
        if (string.IsNullOrEmpty(userTimeZoneId))
        {
            throw new ArgumentNullException(nameof(userTimeZoneId), "Идентификатор часового пояса не может быть пустым.");
        }

        TimeOnly workFromUtc;
        TimeOnly workToUtc;

        if (workFrom != TimeOnly.MinValue && workTo != TimeOnly.MinValue)
        {
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);

            var utcNow = DateTime.UtcNow;
            var userLocalNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, userTimeZone);
            var userLocalDate = userLocalNow.Date;

            var workFromLocal = userLocalDate + workFrom.ToTimeSpan();
            var workToLocal = userLocalDate + workTo.ToTimeSpan();

            var workFromUtcDateTime = TimeZoneInfo.ConvertTimeToUtc(workFromLocal, userTimeZone);
            var workToUtcDateTime = TimeZoneInfo.ConvertTimeToUtc(workToLocal, userTimeZone);

            workFromUtc = TimeOnly.FromDateTime(workFromUtcDateTime);
            workToUtc = TimeOnly.FromDateTime(workToUtcDateTime);
        }
        else
        {
            workFromUtc = TimeOnly.MinValue;
            workToUtc = TimeOnly.MinValue;
        }

        return (workFromUtc, workToUtc);
    }
}