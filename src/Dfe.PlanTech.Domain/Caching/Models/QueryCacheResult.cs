namespace Dfe.PlanTech.Domain.Caching.Models;

public record QueryCacheResult<T>(T? Result, CacheRetrievalSource RetrievedFrom);
