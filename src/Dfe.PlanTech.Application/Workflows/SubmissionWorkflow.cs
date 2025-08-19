using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Application.Workflows;

public class SubmissionWorkflow(
    StoredProcedureRepository storedProcedureRepository,
    SubmissionRepository submissionRepository
)
{
    private readonly StoredProcedureRepository _storedProcedureRepository = storedProcedureRepository ?? throw new ArgumentNullException(nameof(storedProcedureRepository));
    private readonly SubmissionRepository _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));

    public async Task CloneLatestCompletedSubmission(int establishmentId, string sectionId)
    {
        var submissionWithResponses = await _submissionRepository.GetLatestSubmissionAndResponsesAsync(establishmentId, sectionId, isCompletedSubmission: false);
        await _submissionRepository.CloneSubmission(submissionWithResponses);
    }

    public async Task<SqlSubmissionDto?> GetLatestSubmissionWithOrderedResponsesAsync(
        int establishmentId,
        QuestionnaireSectionEntry section,
        bool? isCompletedSubmission
    )
    {
        var latestSubmission = await _submissionRepository.GetLatestSubmissionAndResponsesAsync(establishmentId, section.Sys.Id, isCompletedSubmission);
        if (latestSubmission is null)
        {
            return null;
        }

        latestSubmission.Responses = GetOrderedResponses(latestSubmission.Responses, section).ToList();

        var lastResponseInUserJourney = latestSubmission.Responses.LastOrDefault();
        if (lastResponseInUserJourney is not null)
        {
            var lastSelectedQuestion = section.Questions
                .FirstOrDefault(q => q.Sys.Id.Equals(lastResponseInUserJourney.Question.ContentfulRef))
                    ?? throw new UserJourneyMissingContentException($"Could not find question with database ID {lastResponseInUserJourney.QuestionId} (Contentful ref {lastResponseInUserJourney.Question.ContentfulRef}) in section with ID {section.Sys.Id}", section);

            if (lastSelectedQuestion.Answers.FirstOrDefault(a => a.Sys.Id.Equals(lastResponseInUserJourney.Answer.ContentfulRef)) is null)
            {
                throw new UserJourneyMissingContentException($"Could not find answer with Contentful reference {lastResponseInUserJourney.Answer.ContentfulRef} in question with Contentful reference {lastResponseInUserJourney.Question.ContentfulRef}", section);
            }
        }

        return latestSubmission.AsDto();
    }

    // On the action on the controller, we should redirect to a new route called "GetNextUnansweredQuestionForSection"
    // which will then either redirect to the "GetQuestionBySlug" route or "Check Answers" route
    public async Task<int> SubmitAnswer(int userId, int establishmentId, SubmitAnswerModel answerModel)
    {
        if (answerModel is null)
        {
            throw new InvalidDataException($"{nameof(answerModel)} is null");
        }

        var model = new AssessmentResponseModel(userId, establishmentId, answerModel);
        var responseId = await _storedProcedureRepository.SubmitResponse(model);

        return responseId;
    }

    public async Task<List<SqlSectionStatusDto>> GetSectionStatusesAsync(int establishmentId, IEnumerable<string> sectionIds)
    {
        var sectionIdsInput = string.Join(',', sectionIds);
        var statuses = await _storedProcedureRepository.GetSectionStatusesAsync(sectionIdsInput, establishmentId);
        return statuses.Select(s => s.AsDto()).ToList();
    }

    public async Task<SqlSectionStatusDto> GetSectionSubmissionStatusAsync(int establishmentId, string sectionId, bool isCompleted)
    {
        var latestSubmission = await _submissionRepository
            .GetLatestSubmissionAndResponsesAsync(establishmentId, sectionId, isCompleted);

        if (latestSubmission is not null)
        {
            return new SqlSectionStatusDto
            {
                Completed = latestSubmission.Completed,
                LastMaturity = latestSubmission.Maturity,
                SectionId = latestSubmission.SectionId,
                Status = latestSubmission.Completed ? SubmissionStatus.CompleteReviewed : SubmissionStatus.InProgress
            };
        }

        return new SqlSectionStatusDto
        {
            Completed = false,
            SectionId = sectionId,
            Status = SubmissionStatus.NotStarted
        };
    }

    public async Task SetMaturityAndMarkAsReviewedAsync(int submissionId)
    {
        await _storedProcedureRepository.SetMaturityForSubmissionAsync(submissionId);
        await _submissionRepository.SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(submissionId);
    }

    public async Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId)
    {
        await _submissionRepository.SetLatestSubmissionViewedAsync(establishmentId, sectionId);
    }

    public async Task SetSubmissionReviewedAsync(int submissionId)
    {
        await _submissionRepository.SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(submissionId);
    }

    public Task DeleteSubmissionSoftAsync(int establishmentId, string sectionId)
    {
        return _submissionRepository.SetSubmissionInaccessibleAsync(establishmentId, sectionId);
    }

    public Task DeleteSubmissionSoftAsync(int submissionId)
    {
        return _submissionRepository.SetSubmissionInaccessibleAsync(submissionId);
    }

    public Task DeleteSubmissionHardAsync(int establishmentId, string sectionId)
    {
        return _storedProcedureRepository.HardDeleteCurrentSubmissionAsync(establishmentId, sectionId);
    }

    private static IEnumerable<ResponseEntity> GetOrderedResponses(IEnumerable<ResponseEntity> responses, QuestionnaireSectionEntry section)
    {
        var questionWithAnswerMap = responses
            .OrderByDescending(r => r.DateCreated)
            .GroupBy(r => r.Question.ContentfulRef)
            .ToDictionary(group => group.Key, group => group.First());

        var currentQuestion = section?.Questions.FirstOrDefault();
        while (currentQuestion is not null)
        {
            if (!questionWithAnswerMap.TryGetValue(currentQuestion.Sys.Id ?? string.Empty, out var response))
            {
                break;
            }

            yield return response;

            currentQuestion = currentQuestion.Answers.ToList()
                .Find(a => a.Sys.Id!.Equals(response.Answer.ContentfulRef))?
                .NextQuestion;
        }
    }
}
