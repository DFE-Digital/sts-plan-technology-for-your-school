using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Middleware;

namespace Dfe.PlanTech.Web.Routing;

public static class CheckAnswersOrNextQuestionChecker
{
  public static readonly UserJourneyStatusChecker CheckAnswersOrNextQuestion = new()
  {
    IsMatchingUserJourney = (userJourneyRouter) => userJourneyRouter.Section != null && 
                                                    userJourneyRouter.SectionStatus != null &&
                                                    userJourneyRouter.SectionStatus.Status > Status.NotStarted ? 
                                                    true : throw new InvalidDataException("Should not be null"),
                                                    
    ProcessUserJourneyRouter = async (userJourneyRouter, cancellationToken) =>
    {
      var responses = await userJourneyRouter.GetResponsesQuery.GetLatestResponses(await userJourneyRouter.User.GetEstablishmentId(),
                                                                            userJourneyRouter.Section!.Sys.Id,
                                                                            cancellationToken) ?? throw new InvalidDataException("Missing responses");

      var lastResponseInUserJourney = userJourneyRouter.Section!.GetAttachedQuestions(responses.Responses).Last();
      var lastSelectedAnswer = userJourneyRouter.Section!.Questions.First(question => question.Sys.Id == lastResponseInUserJourney.QuestionRef)
                                                                    .Answers.First(answer => answer.Sys.Id == lastResponseInUserJourney.AnswerRef);

      if (lastSelectedAnswer.NextQuestion == null)
      {
        userJourneyRouter.Status = JourneyStatus.CheckAnswers;
        return;
      }

      userJourneyRouter.Status = JourneyStatus.NextQuestion;
      userJourneyRouter.NextQuestion = lastSelectedAnswer.NextQuestion;
    }
  };
}

