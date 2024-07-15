using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Services;

public class CacheHandler(
    HttpClient httpClient,
    CacheRefreshConfiguration cacheRefreshConfiguration,
    ILogger<CacheHandler> logger) : ICacheHandler
{
    /// <summary>
    /// Makes a call to the plan tech web app that invalidates the database cache.
    /// </summary>
    public async Task RequestCacheClear()
    {
        if (cacheRefreshConfiguration.ApiKeyName is null)
        {
            logger.LogError("No Api Key name has been configured but is required for clearing the website cache");
            return;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, cacheRefreshConfiguration.Endpoint);
        request.Headers.Add(cacheRefreshConfiguration.ApiKeyName, cacheRefreshConfiguration.ApiKeyValue);

        await httpClient.SendAsync(request);
    }
}
