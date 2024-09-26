using Dfe.PlanTech.Web.Helpers;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("cache")]
[LogInvalidModelState]
public class CacheController(ILogger<CacheController> cacheLogger) : BaseController<CacheController>(cacheLogger)
{
    [HttpPost("clear")]
    [ValidateApiKey]
    public IActionResult ClearCache([FromServices] IEFCacheServiceProvider cacheServiceProvider)
    {
        try
        {
            cacheServiceProvider.ClearAllCachedEntries();
            Logger.LogInformation("Database cache has been cleared");
            return Ok(true);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occured while trying to clear the database cache: {message}", e.Message);
            return StatusCode(500, false);
        }
    }
}
