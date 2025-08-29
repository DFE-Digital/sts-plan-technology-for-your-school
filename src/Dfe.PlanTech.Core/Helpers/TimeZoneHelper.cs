namespace Dfe.PlanTech.Core.Helpers;

public static class TimeZoneHelper
{
    public static DateTime ToUkTime(DateTime utcDateTime)
    {
        var britishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, britishTimeZone);
        return localTime;
    }
}
