namespace Dfe.PlanTech.Domain.Helpers;

public static class TimeZoneHelpers
{
    public static DateTime ToUkTime(DateTime utcDateTime)
    {
        var britishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, britishTimeZone);
        return localTime;
    }
}
