using Dfe.PlanTech.Domain.Interfaces;

namespace Dfe.PlanTech.Domain.Helpers;

public class SystemTime : ISystemTime
{
    public DateTime Today => UkNow.Date;

    public DateTime UkNow => TimeZoneHelpers.ToUkTime(DateTime.UtcNow);

    public static DateTime UtcNow => DateTime.UtcNow;
}
