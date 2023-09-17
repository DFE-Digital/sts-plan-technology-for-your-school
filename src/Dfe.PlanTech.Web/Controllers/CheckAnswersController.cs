using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Submissions.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Responses.Interfaces;
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
                            throw new NullReferenceException($"Could not find section for {sectionSlug}");

        var checkAnswerPageContent = await getPageQuery.GetPageBySlug(PAGE_SLUG, CancellationToken.None) ??
                                        throw new NullReferenceException($"Could not find page for slug {PAGE_SLUG}");

        var establishmentId = await user.GetEstablishmentId();

        var responses = await processCheckAnswerDtoCommand.GetCheckAnswerDtoForSectionId(establishmentId, section!.Sys.Id, cancellationToken);

        if (responses == null) return RedirectToAction("GetByRoute", "Pages", new { route = "/self-assessment" });

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
    public async Task<IActionResult> ConfirmCheckAnswers(int submissionId, [FromServices] ICalculateMaturityCommand calculateMaturityCommand, CancellationToken cancellationToken = default)
    {
        await calculateMaturityCommand.CalculateMaturityAsync(submissionId, cancellationToken);

        return RedirectToAction("GetByRoute", "Pages", new { route = "self-assessment" });
    }
}