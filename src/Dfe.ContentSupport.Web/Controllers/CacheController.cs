using Dfe.ContentSupport.Web.Extensions;
using Dfe.ContentSupport.Web.Models.Mapped;
using Dfe.ContentSupport.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.ContentSupport.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CacheController(
    [FromKeyedServices(WebApplicationBuilderExtensions.ContentAndSupportServiceKey)]
    ICacheService<List<CsPage>> cache) : ControllerBase
{
    [HttpGet]
    [Route("clear")]
    public IActionResult Clear()
    {
        cache.ClearCache();

        return Ok();
    }
}
