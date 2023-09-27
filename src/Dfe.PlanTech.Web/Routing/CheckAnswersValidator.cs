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

  public CheckAnswersValidator(IGetPageQuery getPageQuery, IProcessCheckAnswerDtoCommand processCheckAnswerDtoCommand, IUser user)
  {
    _getPageQuery = getPageQuery;
    _processCheckAnswerDtoCommand = processCheckAnswerDtoCommand;
    _user = user;
  }

  public async Task<IActionResult> ValidateRoute(string sectionSlug,
                                                 UserJourneyRouter router,
                                                 CheckAnswersController controller,
                                                 CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentException($"'{nameof(sectionSlug)}' cannot be null or empty.");

    return router.Status switch
    {
      JourneyStatus.CheckAnswers => await ProcessCheckAnswers(sectionSlug, router, controller, cancellationToken),
      JourneyStatus.Completed => controller.RedirectToAction("GetByRoute", "Pages", new { route = sectionSlug }),
      _ => ProcessQuestionStatus(sectionSlug, router, controller),
    };
  }

  private async Task<IActionResult> ProcessCheckAnswers(string sectionSlug, UserJourneyRouter router, CheckAnswersController controller, CancellationToken cancellationToken)
  {
    var establishmentId = await _user.GetEstablishmentId();
    var checkAnswerDto = await _processCheckAnswerDtoCommand.GetCheckAnswerDtoForSection(establishmentId, router.Section!, cancellationToken);

    if (checkAnswerDto == null || checkAnswerDto.Responses == null) return controller.RedirectToSelfAssessment();

    var checkAnswerPageContent = await _getPageQuery.GetPageBySlug(CheckAnswersController.CheckAnswersPageSlug, CancellationToken.None) ??
                                 throw new KeyNotFoundException($"Could not find page for slug {CheckAnswersController.CheckAnswersPageSlug}");

    var model = new CheckAnswersViewModel()
    {
      Title = checkAnswerPageContent.Title ?? new Title() { Text = "Check Answers" },
      SectionName = router.Section!.Name,
      CheckAnswerDto = checkAnswerDto,
      Content = checkAnswerPageContent.Content,
      SectionSlug = sectionSlug,
      SubmissionId = checkAnswerDto.SubmissionId,
      Slug = checkAnswerPageContent.Slug
    };

    return controller.View(CheckAnswersController.CheckAnswersViewName, model);
  }

  private static IActionResult ProcessQuestionStatus(string sectionSlug, UserJourneyRouter router, Controller controller)
  {
    var nextQuestionSlug = router.NextQuestion?.Slug ?? router.Section!.Questions.Select(question => question.Slug).First();

    return controller.RedirectToAction(QuestionsController.GetQuestionBySlugAction,
                                       QuestionsController.Controller,
                                       new { sectionSlug, questionSlug = nextQuestionSlug });
  }
}