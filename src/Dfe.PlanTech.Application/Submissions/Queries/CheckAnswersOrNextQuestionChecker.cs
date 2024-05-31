using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Application.Submissions.Queries;

/// <summary>
/// User journey status checker for when the user is at the Check Answers or their Next Question
/// </summary>
public static class CheckAnswersOrNextQuestionChecker
{
    public static readonly ISubmissionStatusChecker CheckAnswersOrNextQuestion = new SubmissionStatusChecker()
    {
        IsMatchingSubmissionStatusFunc = (userJourneyRouter) => userJourneyRouter.SectionStatus != null &&
                                                                userJourneyRouter.SectionStatus.Status > Status.NotStarted &&
                                                                userJourneyRouter.SectionStatus.Status != Status.Completed,
        ProcessSubmissionFunc = async (userJourneyRouter, cancellationToken) =>
        {
            var responses = await userJourneyRouter.GetResponsesQuery.GetLatestResponses(await userJourneyRouter.User.GetEstablishmentId(),
                                                                                        userJourneyRouter.Section.Sys.Id,
                                                                                        cancellationToken) ?? throw new InvalidDataException("Missing responses");

            var lastResponseInUserJourney = userJourneyRouter.Section.GetOrderedResponsesForJourney(responses.Responses).Last();

            var lastSelectedAnswer = userJourneyRouter.Section.Questions.First(question => question.Sys.Id == lastResponseInUserJourney.QuestionRef)
                                                                      .Answers.First(answer => answer.Sys.Id == lastResponseInUserJourney.AnswerRef);

            if (lastSelectedAnswer.NextQuestion == null)
            {
                userJourneyRouter.Status = SubmissionStatus.CheckAnswers;
                return;
            }

            userJourneyRouter.Status = SubmissionStatus.NextQuestion;
            userJourneyRouter.NextQuestion = GetNextQuestion(userJourneyRouter, lastSelectedAnswer);
        }
    };

    private static Question? GetNextQuestion(ISubmissionStatusProcessor userJourneyRouter, Answer lastSelectedAnswer)
    => userJourneyRouter.Section.Questions.Find(question => question.Sys.Id == lastSelectedAnswer.NextQuestion?.Sys.Id);
}

