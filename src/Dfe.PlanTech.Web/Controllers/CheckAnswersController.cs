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
    public const string PageSlug = "check-answers";

    public const string InlineRecommendationUnavailableErrorMessage =
        "Unable to save. Please try again. If this problem continues you can";

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
        
        var errorMessage = TempData["ErrorMessage"]?.ToString();
        
        var checkAnswersViewModel = await GenerateCheckAnswersViewModel(sectionSlug, errorMessage, cancellationToken);
        
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

            TempData["ErrorMessage"] = InlineRecommendationUnavailableErrorMessage;

            return this.RedirectToCheckAnswers(sectionSlug);
        }
        TempData["SectionName"] = sectionName;
        return this.RedirectToSelfAssessment();
    }
    
    private async Task<CheckAnswersViewModel> GenerateCheckAnswersViewModel(string sectionSlug, string? errorMessage,
        CancellationToken cancellationToken = default)
    {
        var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ??
                      throw new KeyNotFoundException($"Could not find section for {sectionSlug}");

        var checkAnswerPageContent = await _getPageQuery.GetPageBySlug(PageSlug, CancellationToken.None) ??
                                     throw new KeyNotFoundException($"Could not find page for slug {PageSlug}");

        var establishmentId = await _user.GetEstablishmentId();

        var responses =
            await _processCheckAnswerDtoCommand.GetCheckAnswerDtoForSection(establishmentId, section,
                cancellationToken);
        
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
            Slug = checkAnswerPageContent.Slug,
            ErrorMessage = errorMessage
        };

        return checkAnswersViewModel;
    }
}