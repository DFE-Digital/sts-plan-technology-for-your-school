namespace Dfe.PlanTech.Domain.Persistence.Models;

public record CacheRefreshConfiguration(string? Endpoint, string? ApiKeyName, string? ApiKeyValue)
{
    public CacheRefreshConfiguration() : this(null, null, null)
    {

    }
}
