using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Caching;

public class CacheClearer([FromServices] IQueryCacher queryCacher, IDistributedCache cache, ILogger<CacheClearer> logger) : ICacheClearer
{
    /// <summary>
    /// Makes a call to the plan tech web app that invalidates the database cache.
    /// </summary>
    public bool ClearCache()
    {
        try
        {
            queryCacher.ClearCache();
            logger.LogInformation("Database cache has been cleared");
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while trying to clear the database cache: {message}", e.Message);
            return false;
        }
    }
}
