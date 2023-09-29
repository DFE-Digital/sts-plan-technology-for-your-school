using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Submissions.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Authorize]
public class CheckAnswersController : BaseController<CheckAnswersController>
{
    public const string PAGE_SLUG = "check-answers";

    public const string INLINE_RECOMMENDATION_UNAVAILABLE_ERROR_MESSAGE =
        "Unable to determine your recommendation. Please try again.";

    private readonly IUser _user;
    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IProcessCheckAnswerDtoCommand _processCheckAnswerDtoCommand;
    private readonly IGetPageQuery _getPageQuery;
    private readonly ICalculateMaturityCommand _calculateMaturityCommand;

    public CheckAnswersController(IUser user, IGetSectionQuery getSectionQuery,
        IProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand, IGetPageQuery getPageQuery, ICalculateMaturityCommand calculateMaturityCommand,
        ILogger<CheckAnswersController> logger) : base(logger)
    {
        _user = user;
        _getSectionQuery = getSectionQuery;
        _processCheckAnswerDtoCommand = processCheckAnswerDtoCommand;
        _getPageQuery = getPageQuery;
        _calculateMaturityCommand = calculateMaturityCommand;
    }

    [HttpGet("{sectionSlug}/check-answers")]
    public async Task<IActionResult> CheckAnswersPage(string sectionSlug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));

        var checkAnswersViewModel = await GenerateCheckAnswersViewModel(sectionSlug, null, cancellationToken);

        if (checkAnswersViewModel == null) return this.RedirectToSelfAssessment();

        return View("CheckAnswers", checkAnswersViewModel);
    }

    [HttpPost("ConfirmCheckAnswers")]
    public async Task<IActionResult> ConfirmCheckAnswers(string sectionSlug, int submissionId, string sectionName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _calculateMaturityCommand.CalculateMaturityAsync(submissionId, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError("There has been an error while trying to calculate maturity", e);

            var errorCheckAnswersViewModel = await GenerateCheckAnswersViewModel(sectionSlug,
                INLINE_RECOMMENDATION_UNAVAILABLE_ERROR_MESSAGE, cancellationToken);

            if (errorCheckAnswersViewModel == null) return this.RedirectToSelfAssessment();

            return View("CheckAnswers", errorCheckAnswersViewModel);
        }

        TempData["SectionName"] = sectionName;
        return this.RedirectToSelfAssessment();
    }


    private async Task<CheckAnswersViewModel> GenerateCheckAnswersViewModel(string sectionSlug, string? errorMessage,
        CancellationToken cancellationToken = default)
    {
        var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                      throw new KeyNotFoundException($"Could not find section for {sectionSlug}");

        var checkAnswerPageContent = await _getPageQuery.GetPageBySlug(PAGE_SLUG, CancellationToken.None) ??
                                     throw new KeyNotFoundException($"Could not find page for slug {PAGE_SLUG}");

        var establishmentId = await _user.GetEstablishmentId();

        var responses =
            await _processCheckAnswerDtoCommand.GetCheckAnswerDtoForSection(establishmentId, section,
                cancellationToken);

        if (responses == null) return null;

        CheckAnswersViewModel checkAnswersViewModel = new()
        {
            Title = checkAnswerPageContent.Title ?? new Title() { Text = "Check Answers" },
            SectionName = section.Name,
            CheckAnswerDto = responses,
            Content = checkAnswerPageContent.Content,
            SectionSlug = sectionSlug,
            SubmissionId = responses.SubmissionId,
            Slug = checkAnswerPageContent.Slug,
            ErrorMessage = errorMessage
        };

        return checkAnswersViewModel;
    }
}