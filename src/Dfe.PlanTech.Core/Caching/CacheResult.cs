using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Caching;

[ExcludeFromCodeCoverage]
public record CacheResult<T>(bool? ExistedInCache = null, T? CacheValue = default, string? Error = null)
{
    public bool Errored => !string.IsNullOrEmpty(Error);
}
