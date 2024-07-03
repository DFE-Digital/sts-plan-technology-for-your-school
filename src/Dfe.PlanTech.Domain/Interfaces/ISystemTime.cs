namespace Dfe.PlanTech.Domain.Interfaces;

public interface ISystemTime
{
    public DateTime Now { get; }

    public DateTime Today { get; }
}
