using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("cache")]
[LogInvalidModelState]
public class CacheController([FromServices] IQueryCacher queryCacher, ILogger<CacheController> cacheLogger) : BaseController<CacheController>(cacheLogger)
{
    [HttpPost("clear")]
    [ValidateApiKey]
    public IActionResult ClearCache()
    {
        try
        {
            queryCacher.ClearCmsCache();
            logger.LogInformation("Database cache has been cleared");
            return Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while trying to clear the database cache: {message}", e.Message);
            return StatusCode(500, false);
        }
    }
}
