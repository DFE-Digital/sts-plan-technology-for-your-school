using System.Text.Json;
using Dfe.PlanTech.Application.Content.Commands;
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
}
