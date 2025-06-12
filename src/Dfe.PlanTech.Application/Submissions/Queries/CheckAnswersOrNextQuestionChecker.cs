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
        IsMatchingSubmissionStatusFunc = (userJourneyRouter) =>
        {
            var status = userJourneyRouter.SectionStatus?.Status;

            return status == Status.InProgress || status == Status.CompleteNotReviewed;
        },

        ProcessSubmissionFunc = async (userJourneyRouter, cancellationToken) =>
        {
            var establishmentId = await userJourneyRouter.User.GetEstablishmentId();
            var sectionId = userJourneyRouter.Section.Sys.Id;

            var responses = await userJourneyRouter.GetResponsesQuery.GetLatestResponses(establishmentId, sectionId, false, cancellationToken)
                            ?? throw new InvalidDataException("Missing responses");

            var lastResponseInUserJourney = userJourneyRouter.Section
                .GetOrderedResponsesForJourney(responses.Responses)
                .LastOrDefault();

            if (lastResponseInUserJourney == null)
            {
                throw new InvalidDataException("No responses found for section.");
            }

            var lastSelectedQuestion = userJourneyRouter.Section.Questions
                .FirstOrDefault(q => q.Sys.Id == lastResponseInUserJourney.QuestionRef)
                ?? throw new UserJourneyMissingContentException($"Could not find question with ID {lastResponseInUserJourney.QuestionRef}", userJourneyRouter.Section);

            var lastSelectedAnswer = lastSelectedQuestion.Answers
                .FirstOrDefault(a => a.Sys.Id == lastResponseInUserJourney.AnswerRef)
                ?? throw new UserJourneyMissingContentException($"Could not find answer with ID {lastResponseInUserJourney.AnswerRef} in question {lastResponseInUserJourney.QuestionRef}", userJourneyRouter.Section);

            // Route user depending on whether there's a next question
            if (lastSelectedAnswer.NextQuestion == null)
            {
                userJourneyRouter.Status = Status.CompleteNotReviewed;
                return;
            }

            userJourneyRouter.Status = Status.InProgress;
            userJourneyRouter.NextQuestion = GetNextQuestion(userJourneyRouter, lastSelectedAnswer);
        }
    };

    private static Question? GetNextQuestion(ISubmissionStatusProcessor userJourneyRouter, Answer lastSelectedAnswer) =>
        userJourneyRouter.Section.Questions.Find(question => question.Sys.Id == lastSelectedAnswer.NextQuestion?.Sys.Id);
}
