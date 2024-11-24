using System.Text.Json;
using Dfe.PlanTech.Application.Content.Commands;
using Dfe.PlanTech.Application.Content.Queries;
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
    public async Task<IEnumerable<Section>?> GetSections([FromServices] GetEntityFromContentfulQuery getEntityFromContentful)
    {
        var sections = await getEntityFromContentful.GetEntities<Section>(depth: 3);
        return sections?.Select(section => new Section()
        {
            Sys = section.Sys,
            Name = section.Name,
            Questions = section.Questions.Select(question => new Question()
            {
                Sys = question.Sys,
                Text = question.Text,
                Answers = question.Answers.Select(answer => new Answer()
                {
                    Sys = answer.Sys,
                    Text = answer.Text,
                    NextQuestion = answer.NextQuestion != null
                        ? new Question() { Sys = answer.NextQuestion.Sys, Text = answer.NextQuestion.Text }
                        : null
                }).ToList()
            }).ToList()
        });
    }
}
