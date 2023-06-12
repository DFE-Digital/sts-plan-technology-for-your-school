
namespace Dfe.PlanTech.Domain.Caching.Interfaces;

public interface ICacheOptions
{
    public TimeSpan DefaultTimeToLive { get; }
}
