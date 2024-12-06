using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Web.Content;
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
