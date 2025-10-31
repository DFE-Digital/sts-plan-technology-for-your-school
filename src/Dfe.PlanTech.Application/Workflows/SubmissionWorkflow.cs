using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Workflows;

public class SubmissionWorkflow(
    ILogger<SubmissionWorkflow> logger,
    IStoredProcedureRepository storedProcedureRepository,
    ISubmissionRepository submissionRepository
) : ISubmissionWorkflow
{
    private readonly ILogger<SubmissionWorkflow> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IStoredProcedureRepository _storedProcedureRepository = storedProcedureRepository ?? throw new ArgumentNullException(nameof(storedProcedureRepository));
    private readonly ISubmissionRepository _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));

    public async Task<SqlSubmissionDto> CloneLatestCompletedSubmission(int establishmentId, QuestionnaireSectionEntry section)
    {
        var submissionWithResponses = await _submissionRepository.GetLatestSubmissionAndResponsesAsync(establishmentId, section.Id, isCompletedSubmission: true);
        var newSubmission = await _submissionRepository.CloneSubmission(submissionWithResponses);
        newSubmission.Responses = GetOrderedResponses(newSubmission.Responses, section).ToList();

        return newSubmission.AsDto();
    }

    public Task ConfirmCheckAnswersAndUpdateRecommendationsAsync(int establishmentId, int? matEstablishmentId, int submissionId, int userId, QuestionnaireSectionEntry section)
    {
        return _submissionRepository.ConfirmCheckAnswersAndUpdateRecommendationsAsync(establishmentId, matEstablishmentId, submissionId, userId, section);
    }

    public async Task<SqlSubmissionDto> GetSubmissionByIdAsync(int submissionId)
    {
        var submission = await _submissionRepository.GetSubmissionByIdAsync(submissionId);
        return submission is null
            ? throw new InvalidOperationException($"Submission with ID '{submissionId}' not found")
            : submission.AsDto();
    }

    public async Task<SqlSubmissionDto?> GetLatestSubmissionWithOrderedResponsesAsync(
        int establishmentId,
        QuestionnaireSectionEntry section,
        bool? isCompletedSubmission
    )
    {
        var latestSubmission = await _submissionRepository.GetLatestSubmissionAndResponsesAsync(establishmentId, section.Id, isCompletedSubmission);
        if (latestSubmission is null)
        {
            return null;
        }

        latestSubmission.Responses = GetOrderedResponses(latestSubmission.Responses, section).ToList();

        var lastResponseInUserJourney = latestSubmission.Responses.LastOrDefault();
        if (lastResponseInUserJourney is not null)
        {
            var lastSelectedQuestion = section.Questions
                .FirstOrDefault(q => q.Id.Equals(lastResponseInUserJourney.Question.ContentfulRef))
                    ?? throw new UserJourneyMissingContentException($"Could not find question with database ID {lastResponseInUserJourney.QuestionId} (Contentful ref {lastResponseInUserJourney.Question.ContentfulRef}) in section with ID {section.Id}", section);

            if (lastSelectedQuestion.Answers.FirstOrDefault(a => a.Id.Equals(lastResponseInUserJourney.Answer.ContentfulRef)) is null)
            {
                _logger.LogWarning("Could not find answer with Contentful reference {AnswerContentfulRef} in question with Contentful reference {QuestionContentfulRef}", lastResponseInUserJourney.Answer.ContentfulRef, lastResponseInUserJourney.Question.ContentfulRef);
            }
        }

        return latestSubmission.AsDto();
    }

    // On the action on the controller, we should redirect to a new route called "GetNextUnansweredQuestionForSection"
    // which will then either redirect to the "GetQuestionBySlug" route or "Check Answers" route
    public async Task<int> SubmitAnswer(int userId, int activeEstablishmentId, int userEstablishmentId, SubmitAnswerModel answerModel)
    {
        if (answerModel is null)
        {
            throw new InvalidDataException($"{nameof(answerModel)} is null");
        }

        var model = new AssessmentResponseModel(userId, activeEstablishmentId, userEstablishmentId, answerModel);
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

    public Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId)
    {
        return _submissionRepository.SetSubmissionInaccessibleAsync(establishmentId, sectionId);
    }

    public Task SetSubmissionInaccessibleAsync(int submissionId)
    {
        return _submissionRepository.SetSubmissionInaccessibleAsync(submissionId);
    }
    public Task SetSubmissionInProgressAsync(int establishmentId, string sectionId)
    {
        return _submissionRepository.SetSubmissionInProgressAsync(establishmentId, sectionId);
    }

    public Task SetSubmissionInProgressAsync(int submissionId)
    {
        return _submissionRepository.SetSubmissionInProgressAsync(submissionId);
    }

    public Task SetSubmissionDeletedAsync(int establishmentId, string sectionId)
    {
        return _storedProcedureRepository.SetSubmissionDeletedAsync(establishmentId, sectionId);
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
            if (!questionWithAnswerMap.TryGetValue(currentQuestion.Id ?? string.Empty, out var response))
            {
                break;
            }

            yield return response;

            currentQuestion = currentQuestion.Answers.ToList()
                .Find(a => a.Id!.Equals(response.Answer.ContentfulRef))?
                .NextQuestion;
        }
    }
}
