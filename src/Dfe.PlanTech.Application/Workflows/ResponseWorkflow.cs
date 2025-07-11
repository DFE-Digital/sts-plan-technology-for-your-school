using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Data.Contentful.Persistence;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;

namespace Dfe.PlanTech.Application.Workflows;

public class ResponseWorkflow
(
    SectionWorkflow sectionEntryRepository,
    SubmissionRepository submissionRepository
)
{
    private readonly SectionWorkflow _sectionEntryRepository = sectionEntryRepository ?? throw new ArgumentNullException(nameof(sectionEntryRepository));
    private readonly SubmissionRepository _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));

    public async Task<QuestionWithAnswerModel?> GetLatestResponseForQuestion(int establishmentId, string sectionId, string questionId)
    {
        return await _submissionRepository.GetPreviousSubmissionsInDescendingOrder(establishmentId, sectionId, isCompleted: false, includeResponses: true)
            .SelectMany(submission => submission.Responses)
            .Where(response => string.Equals(questionId, response.Question.ContentfulSysId))
            .OrderByDescending(response => response.DateCreated)
            .Select(response => new QuestionWithAnswerModel(response.AsDto()))
            .FirstOrDefaultAsync();
    }

    public async Task<SqlSubmissionDto?> GetLatestSubmission(int establishmentId, string sectionId, bool isCompletedSubmission, bool includeResponses)
    {
        var latestSubmission = await _submissionRepository.GetPreviousSubmissionsInDescendingOrder(
            establishmentId,
            sectionId,
            isCompletedSubmission,
            includeResponses)
            .FirstOrDefaultAsync();

        return latestSubmission?.AsDto();
    }

    public async Task<SqlSubmissionDto?> GetLatestIncompleteSubmissionWithOrderedResponses(int establishmentId, CmsQuestionnaireSectionDto section)
    {
        var latestSubmission = await _submissionRepository.GetPreviousSubmissionsInDescendingOrder(
            establishmentId,
            section.Id,
            isCompleted: false,
            includeResponses: true)
            .FirstOrDefaultAsync();

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

        var lastSelectedAnswer = lastSelectedQuestion.Answers
            .FirstOrDefault(a => a.Id.Equals(lastResponseInUserJourney.AnswerId))
                ?? throw new UserJourneyMissingContentException($"Could not find answer with ID {lastResponseInUserJourney.AnswerId} in question {lastResponseInUserJourney.QuestionId}", section);

        return latestSubmission.AsDto();
    }

    private static IEnumerable<ResponseEntity> GetOrderedResponses(IEnumerable<ResponseEntity> responses, CmsQuestionnaireSectionDto section)
    {
        var questionMap = responses
            .Where(r => !string.IsNullOrWhiteSpace(r.Question.ContentfulSysId))
            .GroupBy(r => r.Question.ContentfulSysId)
            .Select(group => group
                .OrderByDescending(r => r.DateCreated)
                .First())
            .ToDictionary(response => response.Question.ContentfulSysId, response => response);

        var orderedResponses = new List<ResponseEntity>();

        var currentQuestion = section?.Questions.FirstOrDefault();
        while (currentQuestion is not null)
        {
            var questionSysId = currentQuestion.Sys.Id ?? string.Empty;
            if (!questionMap.TryGetValue(questionSysId, out var response))
            {
                break;
            }

            orderedResponses.Add(response);

            currentQuestion = currentQuestion.Answers
                .Find(a => a.Sys.Id!.Equals(response.Answer.ContentfulSysId))?
                .NextQuestion;
        }

        return orderedResponses;
    }
}
