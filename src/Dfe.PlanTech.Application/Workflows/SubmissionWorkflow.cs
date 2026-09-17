using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.Providers.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class SubmissionWorkflow(
    IAnswerRepository answerRepository,
    IEstablishmentRecommendationHistoryRepository establishmentRecommendationHistoryRepository,
    IQuestionRepository questionRepository,
    IRecommendationRepository recommendationRepository,
    IResponseRepository responseRepository,
    ISubmissionRepository submissionRepository,
    IUserActionIdProvider userActionIdProvider
) : ISubmissionWorkflow
{
    private readonly IAnswerRepository _answerRepository =
        answerRepository ?? throw new ArgumentNullException(nameof(answerRepository));
    private readonly IEstablishmentRecommendationHistoryRepository _establishmentRecommendationHistoryRepository =
        establishmentRecommendationHistoryRepository
        ?? throw new ArgumentNullException(nameof(establishmentRecommendationHistoryRepository));
    private readonly IQuestionRepository _questionRepository =
        questionRepository ?? throw new ArgumentNullException(nameof(questionRepository));
    private readonly IRecommendationRepository _recommendationRepository =
        recommendationRepository
        ?? throw new ArgumentNullException(nameof(recommendationRepository));
    private readonly IResponseRepository _responseRepository =
        responseRepository ?? throw new ArgumentNullException(nameof(responseRepository));
    private readonly ISubmissionRepository _submissionRepository =
        submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
    private readonly IUserActionIdProvider _userActionIdProvider =
        userActionIdProvider ?? throw new ArgumentNullException(nameof(userActionIdProvider));

    public async Task<SqlSubmissionDto> CloneLatestCompletedSubmission(
        int establishmentId,
        string sectionId
    )
    {
        var submissionWithResponses =
            await _submissionRepository.GetLatestSubmissionAndResponsesAsync(
                establishmentId,
                sectionId,
                status: SubmissionStatus.CompleteReviewed
            );
        var newSubmission = await _submissionRepository.CloneSubmission(submissionWithResponses);
        newSubmission.Responses = GetOrderedResponses(newSubmission.Responses).ToList();

        return newSubmission.AsDto();
    }

    public async Task ConfirmCheckAnswersAndUpdateRecommendationsAsync(
        int establishmentId,
        int? matEstablishmentId,
        int submissionId,
        int userId,
        QuestionnaireSectionEntry section
    )
    {
        var submission = await _submissionRepository.GetSubmissionByIdWithResponsesAsync(
            submissionId
        );
        if (submission is null)
        {
            throw new InvalidOperationException(
                $"Could not find submission with ID {submissionId} in database"
            );
        }

        var responses = submission
            .Responses.GroupBy(r => r.QuestionId)
            .Select(group => group.OrderByDescending(r => r.DateCreated).First());

        var responseQuestionRefs = responses.ToDictionary(r => r.Id, r => r.Question.ContentfulRef);
        var responseAnswerRefs = responses.ToDictionary(r => r.Id, r => r.Answer.ContentfulRef);

        var questionRefsToRecommendations = section
            .CoreRecommendations.Where(cr => cr.Question is not null)
            .ToDictionary(cr => cr.Question.Id, cr => cr);

        var responseRecommendations = responses.ToDictionary(
            r => r.Id,
            r => questionRefsToRecommendations[r.Question.ContentfulRef]
        );

        var recommendationRefsToResponseIds = responses.ToDictionary(
            r => responseRecommendations[r.Id].Id,
            r => r.Id
        );

        var recommendationDtos = new List<SqlRecommendationDto>();
        var recommendationStatuses = new Dictionary<string, RecommendationStatus>();
        foreach (var response in responses)
        {
            var coreRecommendation = responseRecommendations[response.Id];

            if (
                responseRecommendations[response.Id]
                    .CompletingAnswers.Any(ca =>
                        string.Equals(ca.Id, response.Answer.ContentfulRef)
                    )
            )
            {
                recommendationStatuses.Add(coreRecommendation.Id, RecommendationStatus.Complete);
            }
            else if (
                responseRecommendations[response.Id]
                    .InProgressAnswers.Any(ca =>
                        string.Equals(ca.Id, response.Answer.ContentfulRef)
                    )
            )
            {
                recommendationStatuses.Add(coreRecommendation.Id, RecommendationStatus.InProgress);
            }
            else
            {
                recommendationStatuses.Add(coreRecommendation.Id, RecommendationStatus.NotStarted);
            }

            // Ensure the DB question's Contentful reference is associated with the Contentful Section
            var question = section.Questions.FirstOrDefault(q =>
                string.Equals(q.Id, questionRefsToRecommendations[q.Id].Id)
            );

            if (question is null)
            {
                throw new InvalidOperationException(
                    "Could not find the question associated with the submission in the database"
                );
            }

            recommendationDtos.Add(
                new SqlRecommendationDto
                {
                    ContentfulSysId = coreRecommendation.Id,
                    RecommendationText = coreRecommendation.Header,
                    QuestionId = response.QuestionId,
                    QuestionContentfulRef = response.Question.ContentfulRef,
                }
            );
        }

        var recommendations = await _recommendationRepository.UpsertRecommendations(
            recommendationDtos
        );

        await _establishmentRecommendationHistoryRepository.UpdateRecommendationStatusesAsync(
            establishmentId,
            matEstablishmentId,
            userId,
            recommendationRefsToResponseIds,
            recommendations,
            recommendationStatuses
        );

        await _submissionRepository.SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(
            submissionId
        );
    }

    public async Task<SqlSubmissionDto> GetSubmissionByIdAsync(int submissionId)
    {
        var submission = await _submissionRepository.GetSubmissionByIdAsync(submissionId);
        return submission is null
            ? throw new InvalidOperationException($"Submission with ID '{submissionId}' not found")
            : submission.AsDto();
    }

    public async Task<SqlSubmissionDto?> GetLatestCompletedSubmissionBySectionIdAsync(
        int establishmentId,
        string sectionId
    )
    {
        var submission = await _submissionRepository.GetLatestCompletedSubmissionBySectionIdAsync(
            establishmentId,
            sectionId
        );
        return submission?.AsDto();
    }

    public async Task<SqlSubmissionDto?> GetLatestSubmissionWithOrderedResponsesAsync(
        int establishmentId,
        string sectionId,
        SubmissionStatus? status
    )
    {
        var latestSubmission = await _submissionRepository.GetLatestSubmissionAndResponsesAsync(
            establishmentId,
            sectionId,
            status
        );
        if (latestSubmission is null)
        {
            return null;
        }

        latestSubmission.Responses = GetOrderedResponses(latestSubmission.Responses).ToList();
        return latestSubmission.AsDto();
    }

    // Overload to take multiple statuses to include in query
    public async Task<SqlSubmissionDto?> GetLatestSubmissionWithOrderedResponsesAsync(
        int establishmentId,
        string sectionId,
        IEnumerable<SubmissionStatus> statuses
    )
    {
        var latestSubmission = await _submissionRepository.GetLatestSubmissionAndResponsesAsync(
            establishmentId,
            sectionId,
            statuses
        );
        if (latestSubmission is null)
        {
            return null;
        }

        latestSubmission.Responses = GetOrderedResponses(latestSubmission.Responses).ToList();
        return latestSubmission.AsDto();
    }

    // On the action on the controller, we should redirect to a new route called "GetNextUnansweredQuestionForSection"
    // which will then either redirect to the "GetQuestionBySlug" route or "Check Answers" route.
    public async Task<int> SubmitAnswer(
        int userId,
        int activeEstablishmentId,
        int userEstablishmentId,
        SubmitAnswerModel answerModel
    )
    {
        if (answerModel is null)
        {
            throw new InvalidDataException($"{nameof(answerModel)} is null");
        }

        if (answerModel.Question is null)
        {
            throw new InvalidDataException($"{nameof(answerModel.Question)} cannot be null");
        }

        if (answerModel.ChosenAnswer is null)
        {
            throw new InvalidDataException($"{nameof(answerModel.ChosenAnswer)} cannot be null");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            answerModel.SectionId,
            nameof(answerModel.SectionId)
        );
        ArgumentException.ThrowIfNullOrWhiteSpace(
            answerModel.SectionName,
            nameof(answerModel.SectionName)
        );
        ArgumentException.ThrowIfNullOrWhiteSpace(
            answerModel.Question.Id,
            nameof(answerModel.Question.Id)
        );
        ArgumentException.ThrowIfNullOrWhiteSpace(
            answerModel.Question.Text,
            nameof(answerModel.Question.Text)
        );
        ArgumentException.ThrowIfNullOrWhiteSpace(
            answerModel.ChosenAnswer?.Id,
            nameof(answerModel.ChosenAnswer.Id)
        );
        ArgumentException.ThrowIfNullOrWhiteSpace(
            answerModel.ChosenAnswer.Text,
            nameof(answerModel.ChosenAnswer.Text)
        );

        var submissionId = await _submissionRepository.SelectOrInsertSubmissionIdAsync(
            answerModel.SectionId,
            answerModel.SectionName,
            activeEstablishmentId
        );

        var questionId = await _questionRepository.GetOrCreateQuestionIdAsync(
            answerModel.Question.Id,
            answerModel.Question.Text
        );

        var answerId = await _answerRepository.GetOrCreateAnswerIdAsync(
            answerModel.ChosenAnswer.Id,
            answerModel.ChosenAnswer.Text
        );

        var userActionId = _userActionIdProvider.GetUserActionId();

        return await _responseRepository.SubmitResponseAndReturnId(
            userId,
            userEstablishmentId,
            submissionId,
            questionId,
            answerId,
            userActionId
        );
    }

    public async Task<List<SqlSectionStatusDto>> GetSectionStatusesAsync(
        int establishmentId,
        IEnumerable<string> sectionIds
    )
    {
        var sectionIdsInput = string.Join(',', sectionIds);
        var statuses = await _submissionRepository.GetSectionStatusesAsync(
            sectionIdsInput,
            establishmentId
        );
        return statuses.Select(s => s.AsDto()).ToList();
    }

    public async Task<SqlSectionStatusDto> GetSectionSubmissionStatusAsync(
        int establishmentId,
        string sectionId,
        SubmissionStatus status
    )
    {
        var latestSubmission = await _submissionRepository.GetLatestSubmissionAndResponsesAsync(
            establishmentId,
            sectionId,
            status
        );

        if (latestSubmission is not null)
        {
            return new SqlSectionStatusDto
            {
                SectionId = latestSubmission.SectionId,
                Status = latestSubmission.Status,
            };
        }

        return new SqlSectionStatusDto
        {
            SectionId = sectionId,
            Status = SubmissionStatus.NotStarted,
        };
    }

    public async Task SetSubmissionReviewedAsync(int submissionId)
    {
        await _submissionRepository.SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(
            submissionId
        );
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
        return _submissionRepository.SetSubmissionDeletedAsync(establishmentId, sectionId);
    }

    private static Dictionary<string, ResponseEntity>.ValueCollection GetOrderedResponses(
        IEnumerable<ResponseEntity> responses
    )
    {
        return responses
            .OrderBy(r => r.DateLastUpdated)
            .GroupBy(r => r.Question.ContentfulRef)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(r => r.DateLastUpdated).First()
            )
            .Values;
    }
}
