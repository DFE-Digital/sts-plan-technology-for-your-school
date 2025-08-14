using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Application.Workflows;

public class ResponseWorkflow
(
    SubmissionRepository submissionRepository
)
{
    private readonly SubmissionRepository _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));

    public Task<QuestionWithAnswerModel?> GetLatestResponseForQuestionAsync(int establishmentId, string sectionId, string questionId)
    {
        return _submissionRepository.GetPreviousSubmissionsInDescendingOrder(
                establishmentId,
                sectionId,
                isCompleted:
                false, includeResponses: true
            )
            .SelectMany(submission => submission.Responses)
            .Where(response => string.Equals(questionId, response.Question.ContentfulRef))
            .OrderByDescending(response => response.DateCreated)
            .Select(response => new QuestionWithAnswerModel(response.AsDto()))
            .FirstOrDefaultAsync();
    }

    public async Task<SubmissionResponsesModel?> GetLatestSubmissionWithOrderedResponsesAsync(int establishmentId, CmsQuestionnaireSectionDto section, bool isCompletedSubmission)
    {
        var latestSubmission = await _submissionRepository.GetLatestSubmissionAndResponsesAsync(establishmentId, section.Id, isCompletedSubmission);
        if (latestSubmission is null)
        {
            return null;
        }

        latestSubmission.Responses = GetOrderedResponses(latestSubmission.Responses, section).ToList();

        var lastResponseInUserJourney = latestSubmission.Responses.LastOrDefault();
        if (lastResponseInUserJourney is null)
        {
            throw new InvalidDataException("No responses found for section.");
        }

        var lastSelectedQuestion = section.Questions
            .FirstOrDefault(q => q.Id.Equals(lastResponseInUserJourney.Question.ContentfulRef))
                ?? throw new UserJourneyMissingContentException($"Could not find question with database ID {lastResponseInUserJourney.QuestionId} (Contentful ref {lastResponseInUserJourney.Question.ContentfulRef}) in section with ID {section.Id}", section);

        if (lastSelectedQuestion.Answers.FirstOrDefault(a => a.Id.Equals(lastResponseInUserJourney.Answer.ContentfulRef)) is null)
        {
            throw new UserJourneyMissingContentException($"Could not find answer with Contentful reference {lastResponseInUserJourney.Answer.ContentfulRef} in question with Contentful reference {lastResponseInUserJourney.Question.ContentfulRef}", section);
        }

        return new SubmissionResponsesModel(latestSubmission.AsDto());
    }

    private static IEnumerable<ResponseEntity> GetOrderedResponses(IEnumerable<ResponseEntity> responses, CmsQuestionnaireSectionDto section)
    {
        var questionWithAnswerMap = responses
            .OrderByDescending(r => r.DateCreated)
            .GroupBy(r => r.Question.ContentfulRef)
            .ToDictionary(group => group.Key, group => group.First());

        var currentQuestion = section?.Questions.FirstOrDefault();
        while (currentQuestion is not null)
        {
            if (!questionWithAnswerMap.TryGetValue(currentQuestion.Id ?? string.Empty, out var response))
            {
                break;
            }

            yield return response;

            currentQuestion = currentQuestion.Answers
                .Find(a => a.Id!.Equals(response.Answer.ContentfulRef))?
                .NextQuestion;
        }
    }
}
