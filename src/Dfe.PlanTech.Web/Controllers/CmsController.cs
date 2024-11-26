using System.Text.Json;
using Dfe.PlanTech.Application.Content.Commands;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Authorisation;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("api/cms")]
[LogInvalidModelState]
public class CmsController(ILogger<CmsController> logger) : BaseController<CmsController>(logger)
{
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
    public async Task<IEnumerable<Section?>> GetSections([FromServices] GetSectionQuery getSectionQuery)
    {
        return await getSectionQuery.GetAllSections();
    }
}
