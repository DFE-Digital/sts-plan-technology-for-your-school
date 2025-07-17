using System.Text.Json;
using Dfe.PlanTech.Infrastructure.ServiceBus.Commands;
using Dfe.PlanTech.Web.Attributes;
using Dfe.PlanTech.Web.Authorisation.Policies;
using Dfe.PlanTech.Web.Routing;
using Dfe.PlanTech.Web.ViewModels.QaVisualiser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("api/cms")]
[LogInvalidModelState]
public class CmsController(
    ILogger<CmsController> logger,
    CmsViewBuilder viewBuilder
) : BaseController<CmsController>(logger)
{
    private readonly ILogger<CmsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly CmsViewBuilder _viewBuilder = viewBuilder ?? throw new ArgumentNullException(nameof(viewBuilder));

    [HttpPost("webhook")]
    [ValidateApiKey]
    [Authorize(SignedRequestAuthorisationPolicy.PolicyName)]
    public async Task<IActionResult> WebhookPayload([FromBody] JsonDocument json, [FromServices] IWriteCmsWebhookToQueueCommand writeToQueueCommand)
    {
        try
        {
            var result = await writeToQueueCommand.WriteMessageToQueue(json, HttpContext.Request);
            if (result.Success)
            {
                return Ok();
            }
            return BadRequest(result);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occured while trying to write the message to the queue: {message}", e.Message);
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Returns all sections from the CMS, used by the qa-visualiser
    /// </summary>
    [HttpGet("sections")]
    [ValidateApiKey]
    public async Task<IEnumerable<SectionViewModel?>> GetSections()
    {
        return await _viewBuilder.GetAllSectionsAsync();
    }

    /// <summary>
    /// Returns all recommendation chunks linked to answer Id's from the CMS, used by the qa-visualiser
    /// </summary>
    [HttpGet("chunks/{page}")]
    [ValidateApiKey]
    public async Task<IActionResult> GetChunks(int? page)
    {
        return await _viewBuilder.GetChunks(this, page);
    }
}
