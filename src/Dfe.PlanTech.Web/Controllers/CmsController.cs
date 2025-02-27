using System.Text.Json;
using Dfe.PlanTech.Application.Content.Commands;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Authorisation;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Models.QaVisualiser;
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

    /// <summary>
    /// Returns all recommendation chunks linked to answer Id's from the CMS, used by the qa-visualiser
    /// </summary>
    [HttpGet("chunks/{page}")]
    [ValidateApiKey]
    public async Task<IActionResult> GetChunks(int? page, [FromServices] GetRecommendationQuery getRecommendationQuery)
    {
        var pageNumber = page ?? 1;
        var queryResult = await getRecommendationQuery.GetChunksByPage(pageNumber);

        var resultModel = new PagedResultModel<ChunkAnswerResultModel>
        {
            Page = pageNumber,
            Total = queryResult.Pagination.Total,
            Items = queryResult.Chunks
                .SelectMany(c => c.Answers.Select(a => new
                {
                    a.Sys,
                    c.Header
                }))
                .Select(c => new ChunkAnswerResultModel(c.Sys.Id, c.Header)).ToList()
        };

        return Ok(resultModel);
    }
}
