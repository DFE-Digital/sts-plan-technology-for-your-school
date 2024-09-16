using Dfe.PlanTech.Domain.Interfaces;

namespace Dfe.PlanTech.Domain.Helpers;

public class SystemTime : ISystemTime
{
    public DateTime Today => DateTime.Today;

    public DateTime UkNow => TimeZoneHelpers.ToUkTime(DateTime.UtcNow);
}
