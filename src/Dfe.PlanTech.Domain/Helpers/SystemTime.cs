using Dfe.PlanTech.Domain.Interfaces;

namespace Dfe.PlanTech.Domain.Helpers;

public class SystemTime : ISystemTime
{
    public DateTime Now => DateTime.Now;

    public DateTime Today => DateTime.Today;

    public DateTime UkNow => ToUkTime(DateTime.UtcNow);

    public DateTime ToUkTime(DateTime utcDateTime)
    {
        var britishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, britishTimeZone);
        return localTime;
    }
}
