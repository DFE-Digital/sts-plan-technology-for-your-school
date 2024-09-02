using Dfe.PlanTech.Domain.Exceptions;
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
                                                                                        false,
                                                                                        cancellationToken) ?? throw new InvalidDataException("Missing responses");

            var lastResponseInUserJourney = userJourneyRouter.Section.GetOrderedResponsesForJourney(responses.Responses).Last();

            var lastSelectedQuestion = userJourneyRouter.Section.Questions.FirstOrDefault(question => question.Sys.Id == lastResponseInUserJourney.QuestionRef);

            if (lastSelectedQuestion == null)
            {
                throw new UserJourneyMissingContentException($"Could not find question with ID {lastResponseInUserJourney.QuestionRef}", userJourneyRouter.Section);
            }

            var lastSelectedAnswer = lastSelectedQuestion.Answers.FirstOrDefault(answer => answer.Sys.Id == lastResponseInUserJourney.AnswerRef);

            if (lastSelectedAnswer == null)
            {
                throw new UserJourneyMissingContentException($"Could not find answer with ID {lastResponseInUserJourney.AnswerRef} in question {lastResponseInUserJourney.QuestionRef}", userJourneyRouter.Section);
            }

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

