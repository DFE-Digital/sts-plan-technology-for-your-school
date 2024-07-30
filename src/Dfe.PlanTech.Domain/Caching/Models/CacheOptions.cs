
using Dfe.PlanTech.Domain.Caching.Interfaces;

namespace Dfe.PlanTech.Domain.Caching.Models;

public class CacheOptions : ICacheOptions
{
    public TimeSpan DefaultTimeToLive { get; init; } = TimeSpan.FromMinutes(60);
}
