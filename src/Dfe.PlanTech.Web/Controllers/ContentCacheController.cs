using Dfe.PlanTech.Web.Content;
using Dfe.PlanTech.Web.Models.Content.Mapped;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContentCacheController(
    [FromKeyedServices(ProgramExtensions.ContentAndSupportServiceKey)]
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
