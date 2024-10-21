namespace Dfe.PlanTech.Domain.Interfaces;

public interface ISystemTime
{
    public DateTime Today { get; }

    public DateTime UkNow { get; }

    public DateTime UtcNow { get; }
}
