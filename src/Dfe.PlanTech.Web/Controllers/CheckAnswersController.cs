using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    public const string PAGE_SLUG = "check-answers";

    public CheckAnswersController(ILogger<CheckAnswersController> logger) : base(logger)
    {
    }

    [HttpGet("{sectionSlug}/check-answers")]
    public async Task<IActionResult> CheckAnswersPage(string sectionSlug, [FromServices] IUser user, [FromServices] IGetSectionQuery getSectionQuery, [FromServices] IProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand, [FromServices] IGetPageQuery getPageQuery, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));

        var section = await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                            throw new KeyNotFoundException($"Could not find section for {sectionSlug}");

        var checkAnswerPageContent = await getPageQuery.GetPageBySlug(PAGE_SLUG, CancellationToken.None) ??
                                        throw new KeyNotFoundException($"Could not find page for slug {PAGE_SLUG}");

        var establishmentId = await user.GetEstablishmentId();

        var responses = await processCheckAnswerDtoCommand.GetCheckAnswerDtoForSection(establishmentId, section, cancellationToken);

        if (responses == null || !responses.Responses.Any())
        {
            throw new DatabaseException("Could not retrieve the answered question list");
        }

        CheckAnswersViewModel checkAnswersViewModel = new()
        {
            Title = checkAnswerPageContent.Title ?? new Title() { Text = "Check Answers" },
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
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId, string sectionName, [FromServices] ICalculateMaturityCommand calculateMaturityCommand, CancellationToken cancellationToken = default)
    {
        await calculateMaturityCommand.CalculateMaturityAsync(submissionId, cancellationToken);

        TempData["SectionName"] = sectionName;

        return this.RedirectToSelfAssessment();
    }
}