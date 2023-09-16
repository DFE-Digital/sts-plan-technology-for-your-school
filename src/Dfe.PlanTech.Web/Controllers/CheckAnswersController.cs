using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Constants;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    private readonly GetQuestionQuery _getQuestionnaireQuery;

    public CheckAnswersController(ILogger<CheckAnswersController> logger, GetQuestionQuery getQuestionQuery) : base(logger)
    {
        this._getQuestionnaireQuery = getQuestionQuery;
    }

    [HttpGet("{sectionSlug}/check-answers")]
    public async Task<IActionResult> CheckAnswersPage(string sectionSlug, [FromServices] IUser user, [FromServices] GetSectionQuery getSectionQuery, [FromServices] ProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand, [FromServices] GetPageQuery getPageQuery, CancellationToken cancellationToken = default)
    {
        var section = await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken);

        var checkAnswerPageContent = await getPageQuery.GetPageBySlug("check-answers", CancellationToken.None);

        var establishmentId = await user.GetEstablishmentId();

        var responses = await processCheckAnswerDtoCommand.GetCheckAnswerDtoForSectionId(establishmentId, section!.Sys.Id);

        CheckAnswersViewModel checkAnswersViewModel = new()
        {
            Title = checkAnswerPageContent.Title ?? throw new NullReferenceException(nameof(checkAnswerPageContent.Title)),
            SectionName = section.Name,
            CheckAnswerDto = responses,
            Content = checkAnswerPageContent.Content,
            SectionSlug = sectionSlug,
            SubmissionId = responses.SubmissionId,
            Slug = checkAnswerPageContent.Slug
        };

        return View("CheckAnswers", checkAnswersViewModel);
    }
    
    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId, string sectionName, [FromServices] ProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand)
    {
        await processCheckAnswerDtoCommand.CalculateMaturityAsync(submissionId);

        TempData["SectionName"] = sectionName;
        return RedirectToAction("GetByRoute", "Pages", new { route = "self-assessment" });

    }
}