using Dfe.PlanTech.Web.Helpers;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("cache")]
public class CacheController : BaseController<CacheController>
{
    public CacheController(ILogger<CacheController> logger) : base(logger)
    {
    }

    [HttpPost("clear")]
    [ValidateApiKey]
    public bool ClearCache([FromServices] IEFCacheServiceProvider cacheServiceProvider)
    {
        try
        {
            cacheServiceProvider.ClearAllCachedEntries();
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
