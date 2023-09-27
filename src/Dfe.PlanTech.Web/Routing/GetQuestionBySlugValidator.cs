using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public class GetQuestionBySlugValidator
{
  private readonly IGetLatestResponsesQuery _getResponseQuery;
  private readonly IUser _user;
  public GetQuestionBySlugValidator(IGetLatestResponsesQuery getResponseQuery, IUser user)
  {
    _getResponseQuery = getResponseQuery;
    _user = user;
  }

  public async Task<IActionResult> ValidateRoute(string sectionSlug,
                                                 string questionSlug,
                                                 UserJourneyRouter router,
                                                 QuestionsController controller,
                                                 CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentException($"'{nameof(sectionSlug)}' cannot be null or empty.");
    if (string.IsNullOrEmpty(questionSlug)) throw new ArgumentException($"'{nameof(questionSlug)}' cannot be null or empty.");

    return router.Status switch
    {
      JourneyStatus.CheckAnswers => await ProcessCheckAnswersStatus(sectionSlug, questionSlug, router, controller, cancellationToken),
      JourneyStatus.NextQuestion or JourneyStatus.NotStarted => ProcessQuestionStatus(sectionSlug, questionSlug, router, controller),
      JourneyStatus.Completed => controller.RedirectToAction("GetByRoute", "Pages", new { route = sectionSlug }),
      _ => throw new InvalidDataException($"Invalid journey state - state is {router.Status}"),
    };
  }

  private static IActionResult ProcessQuestionStatus(string sectionSlug, string questionSlug, UserJourneyRouter router, QuestionsController controller)
  {
    if (router.NextQuestion == null)
    {
      throw new InvalidDataException("Next question is null but really shouldn't be");
    }

    if (router.NextQuestion!.Slug != questionSlug)
    {
      return controller.RedirectToAction(QuestionsController.GetQuestionBySlugAction,
                                         new { sectionSlug, questionSlug = router.NextQuestion!.Slug });
    }

    var viewModel = controller.GenerateViewModel(sectionSlug, router.NextQuestion!, router.Section!, null);
    return controller.RenderView(viewModel);
  }

  private async Task<IActionResult> ProcessCheckAnswersStatus(string sectionSlug, string questionSlug, UserJourneyRouter router, QuestionsController controller, CancellationToken cancellationToken)
  {
    var question = router.Section!.Questions.FirstOrDefault(q => q.Slug == questionSlug) ??
                    throw new ContentfulDataUnavailableException("No");

    var latestResponses = await _getResponseQuery.GetLatestResponses(await _user.GetEstablishmentId(),
                                                                              router.Section.Sys.Id,
                                                                              cancellationToken) ??
                        throw new InvalidDataException($"Could not find latest response for {questionSlug} but is in CheckAnswers status");

    var isAttachedQuestion = router.Section.GetAttachedQuestions(latestResponses.Responses)
                                                  .Any(response => response.QuestionRef == question.Sys.Id);

    if (!isAttachedQuestion) return controller.RedirectToCheckAnswers(sectionSlug);

    var latestResponseForQuestion = latestResponses.Responses.First(response => response.QuestionRef == question.Sys.Id);

    var viewModel = controller.GenerateViewModel(sectionSlug,
                                                 question,
                                                 router.Section,
                                                 latestResponseForQuestion.AnswerRef);

    return controller.RenderView(viewModel);
  }
}