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

    public async Task<QuestionWithAnswerModel?> GetLatestResponseForQuestionAsync(int establishmentId, string sectionId, string questionId)
    {
        return await _submissionRepository.GetPreviousSubmissionsInDescendingOrder(establishmentId, sectionId, isCompleted: false, includeResponses: true)
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

        latestSubmission.Responses = GetOrderedResponses(latestSubmission.Responses, section);

        var lastResponseInUserJourney = latestSubmission.Responses.LastOrDefault();
        if (lastResponseInUserJourney is null)
        {
            throw new InvalidDataException("No responses found for section.");
        }

        var lastSelectedQuestion = section.Questions
            .FirstOrDefault(q => q.Id.Equals(lastResponseInUserJourney.QuestionId))
                ?? throw new UserJourneyMissingContentException($"Could not find question with ID {lastResponseInUserJourney.QuestionId} in section with ID {section.Id}", section);

        if (lastSelectedQuestion.Answers.FirstOrDefault(a => a.Id.Equals(lastResponseInUserJourney.AnswerId)) is null)
        {
            throw new UserJourneyMissingContentException($"Could not find answer with ID {lastResponseInUserJourney.AnswerId} in question {lastResponseInUserJourney.QuestionId}", section);
        }

        return new SubmissionResponsesModel(latestSubmission.AsDto());
    }

    private static IEnumerable<ResponseEntity> GetOrderedResponses(IEnumerable<ResponseEntity> responses, CmsQuestionnaireSectionDto section)
    {
        var currentQuestion = section?.Questions.FirstOrDefault();
        var questionMap = responses.ToDictionary(response => response.Question.ContentfulRef, response => response);

        var orderedResponses = new List<ResponseEntity>();
        while (currentQuestion is not null)
        {
            var questionSysId = currentQuestion.Sys.Id ?? string.Empty;
            if (!questionMap.TryGetValue(questionSysId, out var response))
            {
                break;
            }

            orderedResponses.Add(response);

            currentQuestion = currentQuestion.Answers
                .Find(a => a.Sys.Id!.Equals(response.Answer.ContentfulRef))?
                .NextQuestion;
        }

        return orderedResponses;
    }
}
