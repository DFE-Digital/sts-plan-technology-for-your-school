using Dfe.PlanTech.Domain.Interfaces;

namespace Dfe.PlanTech.Domain.Helpers;

public class SystemTime : ISystemTime
{
    public DateTime Now => DateTime.Now;

    public DateTime Today => DateTime.Today;
}
