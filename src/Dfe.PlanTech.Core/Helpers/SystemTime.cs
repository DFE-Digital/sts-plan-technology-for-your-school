namespace Dfe.PlanTech.Domain.Helpers;

public class SystemTime
{
    public DateTime Today => UkNow.Date;

    public DateTime UkNow => TimeZoneHelpers.ToUkTime(DateTime.UtcNow);

    public DateTime UtcNow => DateTime.UtcNow;
}
