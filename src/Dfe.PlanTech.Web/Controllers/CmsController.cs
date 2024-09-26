using System.Text.Json;
using Dfe.PlanTech.Application.Content.Commands;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("api/cms")]
[LogInvalidModelState]
public class CmsController(ILogger<CmsController> logger) : BaseController<CmsController>(logger)
{
    [HttpPost("webhook")]
    [ValidateApiKey]
    public async Task<IActionResult> WebhookPayload([FromBody] JsonDocument json, [FromServices] IWriteCmsWebhookToQueueCommand writeToQueueCommand)
    {
        try
        {
            var result = await writeToQueueCommand.WriteMessageToQueue(json, HttpContext.Request);
            if (result == null)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occured while trying to write the message to the queue: {message}", e.Message);
            return BadRequest(e.Message);
        }
    }
}
