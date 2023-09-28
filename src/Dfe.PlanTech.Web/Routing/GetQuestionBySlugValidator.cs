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
  private readonly UserJourneyRouter _router;

  public GetQuestionBySlugValidator(IGetLatestResponsesQuery getResponseQuery, IUser user, UserJourneyRouter router)
  {
    _getResponseQuery = getResponseQuery;
    _user = user;
    _router = router;
  }

  public async Task<IActionResult> ValidateRoute(string sectionSlug,
                                                 string questionSlug,
                                                 QuestionsController controller,
                                                 CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentException($"'{nameof(sectionSlug)}' cannot be null or empty.");
    if (string.IsNullOrEmpty(questionSlug)) throw new ArgumentException($"'{nameof(questionSlug)}' cannot be null or empty.");

    await _router.GetJourneyStatusForSection(sectionSlug, cancellationToken);

    return _router.Status switch
    {
      JourneyStatus.CheckAnswers => await ProcessCheckAnswersStatus(sectionSlug, questionSlug, controller, cancellationToken),
      JourneyStatus.NextQuestion or JourneyStatus.NotStarted or JourneyStatus.Completed => ProcessQuestionStatus(sectionSlug, questionSlug, controller),
      _ => throw new InvalidDataException($"Invalid journey state - state is {_router.Status}"),
    };
  }

  private IActionResult ProcessQuestionStatus(string sectionSlug, string questionSlug, QuestionsController controller)
  {
    if (_router.NextQuestion == null)
    {
      throw new InvalidDataException("Next question is null but really shouldn't be");
    }

    if (_router.NextQuestion!.Slug != questionSlug)
    {
      return QuestionsController.RedirectToQuestionBySlug(sectionSlug, _router.NextQuestion!.Slug, controller);
    }

    var viewModel = controller.GenerateViewModel(sectionSlug, _router.NextQuestion!, _router.Section!, null);
    return controller.RenderView(viewModel);
  }

  private async Task<IActionResult> ProcessCheckAnswersStatus(string sectionSlug, string questionSlug, QuestionsController controller, CancellationToken cancellationToken)
  {
    var question = _router.Section!.Questions.FirstOrDefault(q => q.Slug == questionSlug) ??
                    throw new ContentfulDataUnavailableException("No");

    var latestResponses = await _getResponseQuery.GetLatestResponses(await _user.GetEstablishmentId(),
                                                                              _router.Section.Sys.Id,
                                                                              cancellationToken) ??
                        throw new InvalidDataException($"Could not find latest response for {questionSlug} but is in CheckAnswers status");

    var isAttachedQuestion = _router.Section.GetAttachedQuestions(latestResponses.Responses)
                                                  .Any(response => response.QuestionRef == question.Sys.Id);

    if (!isAttachedQuestion) return controller.RedirectToCheckAnswers(sectionSlug);

    var latestResponseForQuestion = latestResponses.Responses.First(response => response.QuestionRef == question.Sys.Id);

    var viewModel = controller.GenerateViewModel(sectionSlug,
                                                 question,
                                                 _router.Section,
                                                 latestResponseForQuestion.AnswerRef);

    return controller.RenderView(viewModel);
  }
}