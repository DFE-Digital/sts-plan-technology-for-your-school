using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Caching.Interfaces;

namespace Dfe.PlanTech.Core.Caching;

[ExcludeFromCodeCoverage]
public class CacheOptions : ICacheOptions
{
    public TimeSpan DefaultTimeToLive { get; init; } = TimeSpan.FromMinutes(60);
}
