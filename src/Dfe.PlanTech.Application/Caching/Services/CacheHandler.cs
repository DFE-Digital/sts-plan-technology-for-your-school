using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Caching.Services;

public class CacheHandler(HttpClient httpClient, CacheRefreshConfiguration cacheRefreshConfiguration, ILogger<CacheHandler> logger) : ICacheHandler
{
    /// <summary>
    /// Makes a call to the plan tech web app that invalidates the database cache.
    /// </summary>
    public async Task RequestCacheClear(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(cacheRefreshConfiguration.ApiKeyName))
        {
            logger.LogError("No Api Key name has been configured but is required for clearing the website cache");
            return;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, cacheRefreshConfiguration.Endpoint);
        request.Headers.Add(cacheRefreshConfiguration.ApiKeyName, cacheRefreshConfiguration.ApiKeyValue);
        try
        {
            await httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error requesting a cache clear after content update");
        }
    }
}
