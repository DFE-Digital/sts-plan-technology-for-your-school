using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public class CheckAnswersValidator
{
  private readonly IGetPageQuery _getPageQuery;
  private readonly IProcessCheckAnswerDtoCommand _processCheckAnswerDtoCommand;
  private readonly IUser _user;
  private readonly UserJourneyRouter _router;

  public CheckAnswersValidator(IGetPageQuery getPageQuery, IProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand, IUser user, UserJourneyRouter router)
  {
    _getPageQuery = getPageQuery;
    _processCheckAnswerDtoCommand = processCheckAnswerDtoCommand;
    _user = user;
    _router = router;
  }

  public async Task<IActionResult> ValidateRoute(string sectionSlug,
                                                 CheckAnswersController controller,
                                                 CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentException($"'{nameof(sectionSlug)}' cannot be null or empty.");

    await _router.GetJourneyStatusForSection(sectionSlug, cancellationToken);

    return _router.Status switch
    {
      JourneyStatus.CheckAnswers => await ProcessCheckAnswers(sectionSlug, controller, cancellationToken),
      JourneyStatus.Completed => controller.RedirectToAction(PagesController.GetPageByRouteAction, PagesController.Controller, new { route = sectionSlug }),
      _ => ProcessQuestionStatus(sectionSlug, controller),
    };
  }

  private async Task<IActionResult> ProcessCheckAnswers(string sectionSlug, CheckAnswersController controller, CancellationToken cancellationToken)
  {
    var establishmentId = await _user.GetEstablishmentId();
    var checkAnswerDto = await _processCheckAnswerDtoCommand.GetCheckAnswerDtoForSection(establishmentId, _router.Section!, cancellationToken);

    if (checkAnswerDto == null || checkAnswerDto.Responses == null) return controller.RedirectToSelfAssessment();

    var checkAnswerPageContent = await _getPageQuery.GetPageBySlug(CheckAnswersController.CheckAnswersPageSlug, CancellationToken.None) ??
                                 throw new KeyNotFoundException($"Could not find page for slug {CheckAnswersController.CheckAnswersPageSlug}");

    var model = new CheckAnswersViewModel()
    {
      Title = checkAnswerPageContent.Title ?? new Title() { Text = "Check Answers" },
      SectionName = _router.Section!.Name,
      CheckAnswerDto = checkAnswerDto,
      Content = checkAnswerPageContent.Content,
      SectionSlug = sectionSlug,
      SubmissionId = checkAnswerDto.SubmissionId,
      Slug = checkAnswerPageContent.Slug
    };

    return controller.View(CheckAnswersController.CheckAnswersViewName, model);
  }

  private IActionResult ProcessQuestionStatus(string sectionSlug, Controller controller)
  {
    var nextQuestionSlug = _router.NextQuestion?.Slug ?? _router.Section!.Questions.Select(question => question.Slug).First();

    return QuestionsController.RedirectToQuestionBySlug(sectionSlug, nextQuestionSlug, controller);
  }
}