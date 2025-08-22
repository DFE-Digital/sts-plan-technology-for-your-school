using Dfe.PlanTech.Core.Caching.Interfaces;

namespace Dfe.PlanTech.Core.Caching;

public class CacheOptions : ICacheOptions
{
    public TimeSpan DefaultTimeToLive { get; init; } = TimeSpan.FromMinutes(60);
}
