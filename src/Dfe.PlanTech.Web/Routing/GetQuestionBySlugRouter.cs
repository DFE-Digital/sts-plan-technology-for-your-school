using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

public class GetQuestionBySlugRouter : IGetQuestionBySlugRouter
{
  private readonly IGetLatestResponsesQuery _getResponseQuery;
  private readonly IUser _user;
  private readonly ISubmissionStatusProcessor _router;

  public GetQuestionBySlugRouter(IGetLatestResponsesQuery getResponseQuery, IUser user, ISubmissionStatusProcessor router)
  {
    _getResponseQuery = getResponseQuery;
    _user = user;
    _router = router;
  }

  public async Task<IActionResult> ValidateRoute(string sectionSlug, string questionSlug, QuestionsController controller, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(sectionSlug)) throw new ArgumentNullException(nameof(sectionSlug));
    if (string.IsNullOrEmpty(questionSlug)) throw new ArgumentNullException(nameof(questionSlug));

    await _router.GetJourneyStatusForSection(sectionSlug, cancellationToken);

    return _router.Status switch
    {
      SubmissionStatus.CheckAnswers => await ProcessCheckAnswersStatus(sectionSlug, questionSlug, controller, cancellationToken),
      SubmissionStatus.NotStarted or SubmissionStatus.NextQuestion or SubmissionStatus.Completed => await ProcessQuestionStatus(sectionSlug, questionSlug, controller, cancellationToken),
      _ => throw new InvalidDataException($"Invalid journey state - state is {_router.Status}"),
    };
  }

  /// <summary>
  /// Return Question page for question slug, if it is the next unanswered question in their section,
  /// if not redirect to that question
  /// </summary>
  /// <param name="sectionSlug"></param>
  /// <param name="questionSlug"></param>
  /// <param name="controller"></param>
  /// <returns></returns>
  /// <exception cref="InvalidDataException"></exception>
  private async Task<IActionResult> ProcessQuestionStatus(string sectionSlug, string questionSlug, QuestionsController controller, CancellationToken cancellationToken)
  {
    if (_router.NextQuestion == null) throw new InvalidDataException($"Next question is null, but shouldn't be for status '{_router.Status}'");

    if (_router.NextQuestion!.Slug == questionSlug)
    {
      var viewModel = controller.GenerateViewModel(sectionSlug, _router.NextQuestion!, _router.Section!, null);
      return controller.RenderView(viewModel);
    }

    if (_router.Status != SubmissionStatus.NextQuestion) return QuestionsController.RedirectToQuestionBySlug(sectionSlug, _router.NextQuestion!.Slug, controller);

    var question = _router.Section!.Questions.FirstOrDefault(q => q.Slug == questionSlug) ??
                   throw new ContentfulDataUnavailableException($"Could not find question '{questionSlug}' in section '{sectionSlug}'");

    var latestResponses = await _getResponseQuery.GetLatestResponses(await _user.GetEstablishmentId(),
                                                                              _router.Section.Sys.Id,
                                                                              cancellationToken) ??
                        throw new InvalidDataException($"Could not find latest response for '{questionSlug}' but is in CheckAnswers status");

    var isAttachedQuestion = _router.Section.GetAttachedQuestions(latestResponses.Responses)
                                                  .Any(response => response.QuestionRef == question.Sys.Id);

    if (!isAttachedQuestion) return QuestionsController.RedirectToQuestionBySlug(sectionSlug, questionSlug, controller);

    var latestResponseForQuestion = latestResponses.Responses.FirstOrDefault(response => response.QuestionRef == question.Sys.Id) ??
                                    throw new InvalidDataException($"Could not find response for question '{question.Sys.Id}'");

    return controller.RenderView(controller.GenerateViewModel(sectionSlug,
                                                 question,
                                                 _router.Section,
                                                 latestResponseForQuestion.AnswerRef));
  }

  /// <summary>
  /// If the question is an attached question in the establishment's latest user journey for the section, return the question page for it,
  /// otherwise redirect to Check Answers page
  /// </summary>
  /// <param name="sectionSlug"></param>
  /// <param name="questionSlug"></param>
  /// <param name="controller"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="ContentfulDataUnavailableException"></exception>
  /// <exception cref="InvalidDataException"></exception>
  private async Task<IActionResult> ProcessCheckAnswersStatus(string sectionSlug, string questionSlug, QuestionsController controller, CancellationToken cancellationToken)
  {
    var question = _router.Section!.Questions.FirstOrDefault(q => q.Slug == questionSlug) ??
                    throw new ContentfulDataUnavailableException($"Could not find question '{questionSlug}' in section '{sectionSlug}'");

    var latestResponses = await _getResponseQuery.GetLatestResponses(await _user.GetEstablishmentId(),
                                                                              _router.Section.Sys.Id,
                                                                              cancellationToken) ??
                        throw new InvalidDataException($"Could not find latest response for '{questionSlug}' but is in CheckAnswers status");

    var isAttachedQuestion = _router.Section.GetAttachedQuestions(latestResponses.Responses)
                                                  .Any(response => response.QuestionRef == question.Sys.Id);

    if (!isAttachedQuestion) return controller.RedirectToCheckAnswers(sectionSlug);

    var latestResponseForQuestion = latestResponses.Responses.FirstOrDefault(response => response.QuestionRef == question.Sys.Id) ??
                                    throw new InvalidDataException($"Could not find response for question '{question.Sys.Id}'");

    var viewModel = controller.GenerateViewModel(sectionSlug,
                                                 question,
                                                 _router.Section,
                                                 latestResponseForQuestion.AnswerRef);

    return controller.RenderView(viewModel);
  }
}
