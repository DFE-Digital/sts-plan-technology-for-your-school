namespace Dfe.PlanTech.Domain.Caching.Models;

public record CacheResult<T>(bool? ExistedInCache = null, T? CacheValue = default, string? Error = null);
