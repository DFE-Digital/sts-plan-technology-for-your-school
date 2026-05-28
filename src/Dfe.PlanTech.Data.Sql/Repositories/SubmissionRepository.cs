using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Interfaces;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class SubmissionRepository(
    PlanTechDbContext dbContext,
    IUserActionIdAccessor userActionIdAccessor) : ISubmissionRepository
{
    protected readonly PlanTechDbContext _db = dbContext;
    private readonly IUserActionIdAccessor _userActionIdAccessor = userActionIdAccessor;

    public async Task<SubmissionEntity> CloneSubmission(SubmissionEntity? existingSubmission)
    {
        ArgumentNullException.ThrowIfNull(existingSubmission);

        var userActionId = _userActionIdAccessor.GetUserActionId();

        var newSubmission = new SubmissionEntity
        {
            SectionId = existingSubmission.SectionId,
            SectionName = existingSubmission.SectionName,
            EstablishmentId = existingSubmission.EstablishmentId,
            DateCreated = DateTime.UtcNow,
            Status = SubmissionStatus.InProgress,
            CreatedUserActionId = userActionId,
            LastUpdatedUserActionId = userActionId,
            Responses = existingSubmission
                .Responses.Select(r => new ResponseEntity
                {
                    QuestionId = r.QuestionId,
                    AnswerId = r.AnswerId,
                    UserId = r.UserId,
                    UserEstablishmentId = r.UserEstablishmentId,
                    Question = r.Question,
                    Answer = r.Answer,
                    DateCreated = DateTime.UtcNow,
                })
                .ToList(),
        };

        await _db.Submissions.AddAsync(newSubmission);
        await _db.SaveChangesAsync();

        return newSubmission;
    }

    public async Task ConfirmCheckAnswersAndUpdateRecommendationsAsync(
        int establishmentId,
        int? matEstablishmentId,
        int submissionId,
        int userId,
        QuestionnaireSectionEntry section
    )
    {
        var submission = await GetSubmissionByIdWithResponsesAsync(submissionId);

        if (submission is null)
        {
            throw new InvalidOperationException(
                $"Could not find submission with ID {submissionId} in database"
            );
        }

        var sectionQuestions = await GetQuestionsForSection(section);

        var recommendationDtos = section
            .CoreRecommendations.Where(r => r.Question is not null)
            .Select(r =>
            {
                var question = sectionQuestions.FirstOrDefault(q =>
                    string.Equals(q.ContentfulRef, r.Question.Id)
                );

                if (question is null)
                {
                    throw new InvalidOperationException(
                        "Could not find the question identified in the submission"
                    );
                }

                return new SqlRecommendationDto
                {
                    RecommendationText = r.Header,
                    ContentfulSysId = r.Id,
                    QuestionId = question.Id,
                    QuestionContentfulRef = question.ContentfulRef,
                };
            });

        var recommendations = await UpsertRecommendations(recommendationDtos);

        var responses = submission.Responses.Select(r => r.Answer.ContentfulRef).ToHashSet();

        var answerStatusDictionary = section
            .CoreRecommendations.Select(r =>
            {
                if (r.CompletingAnswers.Any(ca => responses.Contains(ca.Id)))
                {
                    return new { r.Id, Status = RecommendationStatus.Complete.ToString() };
                }

                if (r.InProgressAnswers.Any(ca => responses.Contains(ca.Id)))
                {
                    return new { r.Id, Status = RecommendationStatus.InProgress.ToString() };
                }

                return new { r.Id, Status = RecommendationStatus.NotStarted.ToString() };
            })
            .Where(x => x is not null)
            .ToDictionary(x => x!.Id, x => x.Status);

        var previousStatuses = await _db
            .EstablishmentRecommendationHistories.Where(erh =>
                erh.EstablishmentId == establishmentId
                && erh.MatEstablishmentId == matEstablishmentId
            )
            .GroupBy(erh => erh.RecommendationId, erh => erh)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.OrderByDescending(erh => erh.DateCreated).First().NewStatus
            );

        var recommendationStatuses = recommendations.Select(
            r => new EstablishmentRecommendationHistoryEntity
            {
                EstablishmentId = establishmentId,
                MatEstablishmentId = matEstablishmentId,
                RecommendationId = r.Id,
                UserId = userId,
                PreviousStatus = previousStatuses.TryGetValue(r.Id, out var previousStatus)
                    ? previousStatus
                    : null,
                NewStatus = answerStatusDictionary[r.ContentfulRef],
            }
        );

        await _db.EstablishmentRecommendationHistories.AddRangeAsync(recommendationStatuses);

        await SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(
            submissionId
        );
    }

    public async Task<SubmissionEntity?> GetLatestSubmissionAndResponsesAsync(
        int establishmentId,
        string sectionId,
        SubmissionStatus? status
    )
    {
        var submission = await GetPreviousSubmissionsInDescendingOrder(
                establishmentId,
                sectionId,
                status
            )
            .FirstOrDefaultAsync();

        if (submission is null)
            return null;

        submission.Responses = submission
            .Responses.OrderByDescending(response => response.DateLastUpdated)
            .GroupBy(response => response.QuestionId)
            .Select(group =>
                group
                    .OrderByDescending(response => response.DateLastUpdated)
                    .ThenByDescending(response => response.Id)
                    .First()
            )
            .ToList();

        return submission;
    }

    // Overload that allows for multiple statuses to be returned from the query
    public async Task<SubmissionEntity?> GetLatestSubmissionAndResponsesAsync(
        int establishmentId,
        string sectionId,
        IEnumerable<SubmissionStatus> statuses
    )
    {
        // Get latest submission
        var submission = await GetPreviousSubmissionsInDescendingOrder(
                establishmentId,
                sectionId,
                statuses
            )
            .FirstOrDefaultAsync();

        if (submission is null)
            return null;

        submission.Responses = submission
            .Responses.OrderByDescending(response => response.DateLastUpdated)
            .GroupBy(response => response.QuestionId)
            .Select(group =>
                group
                    .OrderByDescending(response => response.DateLastUpdated)
                    .ThenByDescending(response => response.Id)
                    .First()
            )
            .ToList();

        return submission;
    }

    public Task<SubmissionEntity?> GetSubmissionByIdAsync(int submissionId)
    {
        return GetSubmissionsBy(s => s.Id == submissionId).FirstOrDefaultAsync();
    }

    public async Task<SubmissionEntity?> GetSubmissionByIdWithResponsesAsync(int submissionId)
    {
        var submission = await GetSubmissionByIdAsync(submissionId);

        if (submission is null)
        {
            return null;
        }

        submission.Responses = submission
            .Responses.OrderByDescending(response => response.DateCreated)
            .GroupBy(response => response.QuestionId)
            .Select(group => group.OrderByDescending(response => response.DateCreated).First())
            .ToList();

        return submission;
    }

    public async Task<SubmissionEntity> SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(
        int submissionId
    )
    {
        var submission = await GetSubmissionByIdAsync(submissionId);
        if (submission is null)
        {
            throw new InvalidOperationException($"Submission not found for ID '{submissionId}'");
        }

        var userActionId = _userActionIdAccessor.GetUserActionId();

        submission.DateCompleted = DateTime.UtcNow;
        submission.CompletedUserActionId = userActionId;
        submission.LastUpdatedUserActionId = userActionId;
        submission.Status = SubmissionStatus.CompleteReviewed;

        var otherSubmissions = await _db
            .Submissions.Where(s =>
                s.Id != submission.Id
                && s.EstablishmentId == submission.EstablishmentId
                && string.Equals(s.SectionId, submission.SectionId)
                && s.Status == SubmissionStatus.CompleteReviewed
            )
            .ToListAsync();

        foreach (var oldSubmissions in otherSubmissions)
        {
            oldSubmissions.Status = SubmissionStatus.Inaccessible;
            oldSubmissions.Deleted = true;
        }

        await _db.SaveChangesAsync();

        return submission;
    }

    public async Task SetSubmissionInaccessibleAsync(int establishmentId, string sectionId)
    {
        var query = GetPreviousSubmissionsInDescendingOrder(
            establishmentId,
            sectionId,
            statuses: [ SubmissionStatus.InProgress, SubmissionStatus.Inaccessible ]
        );

        var submission =
            await query.FirstOrDefaultAsync()
            ?? throw new InvalidOperationException(
                $"Submission not found for establishment ID '{establishmentId}' and section ID '{sectionId}'"
            );

        await SetSubmissionInaccessibleAsync(submission.Id);
    }

    public async Task<SubmissionEntity> SetSubmissionInaccessibleAsync(int submissionId)
    {
        var submission = await GetSubmissionByIdAsync(submissionId);
        if (submission is null)
        {
            throw new InvalidOperationException($"Submission not found for ID '{submissionId}'");
        }

        submission.Status = SubmissionStatus.Inaccessible;
        submission.LastUpdatedUserActionId = _userActionIdAccessor.GetUserActionId();

        await _db.SaveChangesAsync();

        return submission;
    }

    public async Task SetSubmissionInProgressAsync(int establishmentId, string sectionId)
    {
        var query = GetPreviousSubmissionsInDescendingOrder(
            establishmentId,
            sectionId,
            statuses: [ SubmissionStatus.InProgress, SubmissionStatus.Inaccessible ]
        );

        var submission = await query.FirstOrDefaultAsync();
        if (submission is null)
        {
            throw new InvalidOperationException(
                $"Submission not found for establishment ID '{establishmentId}' and section ID '{sectionId}'"
            );
        }

        await SetSubmissionInProgressAsync(submission.Id);
    }

    public async Task<SubmissionEntity> SetSubmissionInProgressAsync(int submissionId)
    {
        var submission = await GetSubmissionByIdAsync(submissionId);
        if (submission is null)
        {
            throw new InvalidOperationException($"Submission not found for ID '{submissionId}'");
        }

        if (submission.Status.Equals(SubmissionStatus.Inaccessible))
        {
            submission.Status = SubmissionStatus.InProgress;
            submission.LastUpdatedUserActionId = _userActionIdAccessor.GetUserActionId();

            await _db.SaveChangesAsync();
        }

        return submission;
    }

    private IQueryable<SubmissionEntity> GetPreviousSubmissionsInDescendingOrder(
        int establishmentId,
        string sectionId,
        SubmissionStatus? status
    )
    {
        return GetSubmissionsBy(submission =>
                !submission.Deleted
                && submission.EstablishmentId == establishmentId
                && submission.SectionId == sectionId
                && (status == null || submission.Status == status)
            )
            .OrderByDescending(submission => submission.DateCreated);
    }

    // Overload that returns submissions with any of the specified statuses
    private IQueryable<SubmissionEntity> GetPreviousSubmissionsInDescendingOrder(
        int establishmentId,
        string sectionId,
        IEnumerable<SubmissionStatus> statuses
    )
    {
        ArgumentNullException.ThrowIfNull(statuses);

        var statusOptions = statuses.ToList();

        if (statusOptions.Count == 0)
            throw new ArgumentException("At least one submission status must be provided", nameof(statuses));

        return GetSubmissionsBy(submission =>
                !submission.Deleted
                && submission.EstablishmentId == establishmentId
                && submission.SectionId == sectionId
                && statusOptions.Contains(submission.Status)
            )
            .OrderByDescending(submission => submission.DateCreated);
    }

    private IQueryable<SubmissionEntity> GetSubmissionsBy(
        Expression<Func<SubmissionEntity, bool>> predicate
    )
    {
        var query = _db
            .Submissions.Where(predicate)
            .Include(s => s.Establishment)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Question)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Answer);

        return query;
    }

    public async Task<List<QuestionEntity>> GetQuestionsForSection(
        QuestionnaireSectionEntry section
    )
    {
        var sectionQuestionRefs = section.Questions.Select(q => q.Sys?.Id).ToList();

        var sectionQuestions = await _db
            .Questions.Where(question => sectionQuestionRefs.Contains(question.ContentfulRef))
            .ToListAsync();

        return sectionQuestions;
    }

    private async Task<List<RecommendationEntity>> UpsertRecommendations(
        IEnumerable<SqlRecommendationDto> recommendationDtos
    )
    {
        var contentfulRefs = recommendationDtos.Select(r => r.ContentfulSysId);
        var existingRecommendations = await _db
            .Recommendations.Where(recommendation =>
                contentfulRefs.Contains(recommendation.ContentfulRef)
            )
            .Where(recommendation => recommendation != null)
            .GroupBy(recommendation => recommendation.ContentfulRef)
            .Select(group => group.OrderByDescending(g => g.DateCreated).First())
            .ToListAsync();

        var existingRecommendationContentfulRefs = existingRecommendations
            .Select(r => r.ContentfulRef)
            .ToList();

        var recommendationEntitiesToInsert = recommendationDtos
            .Where(rm => !existingRecommendationContentfulRefs.Contains(rm.ContentfulSysId))
            .Select(BuildRecommendationEntity)
            .ToList();

        var recommendationDtoDictionary = recommendationDtos.ToDictionary(
            r => r.ContentfulSysId,
            r => r
        );
        var recommendationsWithNoChanges = new List<RecommendationEntity>();

        foreach (var existingRecommendation in existingRecommendations)
        {
            recommendationDtoDictionary.TryGetValue(
                existingRecommendation.ContentfulRef,
                out var recommendationDto
            );
            if (recommendationDto is null)
            {
                continue;
            }

            if (
                !string.Equals(
                    recommendationDto.RecommendationText,
                    existingRecommendation.RecommendationText
                )
            )
            {
                recommendationEntitiesToInsert.Add(BuildRecommendationEntity(recommendationDto));
            }
            else
            {
                recommendationsWithNoChanges.Add(BuildRecommendationEntity(recommendationDto));
            }
        }

        _db.AddRange(recommendationEntitiesToInsert);

        await _db.SaveChangesAsync();

        return await _db
            .Recommendations.Where(r => contentfulRefs.Contains(r.ContentfulRef))
            .ToListAsync();
    }

    public async Task<List<SectionStatusEntity>> GetSectionStatusesAsync(
        string sectionIds,
        int establishmentId
    )
    {
        var sectionIdList = sectionIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var currentSubmissions = await _db
            .Submissions.Where(s =>
                !s.Deleted
                && s.EstablishmentId == establishmentId
                && sectionIdList.Contains(s.SectionId)
            )
            .GroupBy(s => s.SectionId)
            .Select(g => g.OrderByDescending(s => s.DateCreated).First())
            .ToListAsync();

        var lastCompleteSubmissions = await _db
            .Submissions.Where(s =>
                !s.Deleted
                && s.EstablishmentId == establishmentId
                && sectionIdList.Contains(s.SectionId)
                && (s.Status == SubmissionStatus.CompleteReviewed)
            )
            .GroupBy(s => s.SectionId)
            .Select(g => g.OrderByDescending(s => s.DateCreated).First())
            .ToListAsync();

        var currentBySectionId = currentSubmissions.ToDictionary(s => s.SectionId, s => s);
        var lastCompleteBySectionId = lastCompleteSubmissions.ToDictionary(
            s => s.SectionId,
            s => s
        );

        var result = sectionIdList
            .Select(sectionId =>
            {
                currentBySectionId.TryGetValue(sectionId, out var currentSubmission);
                lastCompleteBySectionId.TryGetValue(sectionId, out var lastCompleteSubmission);

                return new SectionStatusEntity
                {
                    SectionId = sectionId,
                    Status = currentSubmission?.Status ?? SubmissionStatus.NotStarted,
                    DateCreated = currentSubmission?.DateCreated ?? DateTime.UtcNow,
                    DateUpdated =
                        currentSubmission?.DateLastUpdated
                        ?? currentSubmission?.DateCreated
                        ?? DateTime.UtcNow,
                    LastCompletionDate = lastCompleteSubmission?.DateCompleted,
                };
            })
            .ToList();

        return result;
    }

    public async Task SetSubmissionDeletedAsync(int establishmentId, string sectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionId);

        var submissionId = await _db
            .Submissions.Where(s =>
                s.SectionId == sectionId
                && s.EstablishmentId == establishmentId
                && s.Status != SubmissionStatus.Inaccessible
            )
            .OrderByDescending(s => s.Id)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();

        if (submissionId is null)
            return;

        var userActionId = _userActionIdAccessor.GetUserActionId();

        await _db
            .Submissions.Where(s => s.Id == submissionId.Value)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Deleted, true)
                .SetProperty(s => s.DateLastUpdated, DateTime.UtcNow)
                .SetProperty(s => s.LastUpdatedUserActionId, userActionId));
    }

    public async Task<int> SubmitResponse(AssessmentResponseModel response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.Answer is null)
        {
            throw new InvalidDataException($"{nameof(response.Answer)} cannot be null");
        }

        if (response.Question is null)
        {
            throw new InvalidDataException($"{nameof(response.Question)} cannot be null");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(response.SectionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(response.SectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(response.Question.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(response.Question.Text);
        ArgumentException.ThrowIfNullOrWhiteSpace(response.Answer.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(response.Answer.Text);

        var submissionId = await SelectOrInsertSubmissionIdAsync(
            response.SectionId,
            response.SectionName,
            response.EstablishmentId);

        var answerId = await GetOrCreateAnswerIdAsync(
            response.Answer.Id,
            response.Answer.Text);

        var questionId = await GetOrCreateQuestionIdAsync(
            response.Question.Id,
            response.Question.Text);

        var responseEntity = new ResponseEntity
        {
            UserId = response.UserId,
            UserEstablishmentId = response.UserEstablishmentId,
            SubmissionId = submissionId,
            QuestionId = questionId,
            AnswerId = answerId,
            DateCreated = DateTime.UtcNow,
        };

        var userActionId = _userActionIdAccessor.GetUserActionId();

        await _db.Responses.AddAsync(responseEntity);

        await _db.Submissions
            .Where(s => s.Id == submissionId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.DateLastUpdated, DateTime.UtcNow)
                .SetProperty(s => s.LastUpdatedUserActionId, userActionId));

        await _db.SaveChangesAsync();

        return responseEntity.Id;
    }

    private async Task<int> SelectOrInsertSubmissionIdAsync(
        string sectionId,
        string sectionName,
        int establishmentId)
    {
        var submissionId = await GetCurrentSubmissionIdAsync(sectionId, establishmentId);

        if (submissionId.HasValue)
        {
            return submissionId.Value;
        }

        var userActionId = _userActionIdAccessor.GetUserActionId();

        var submission = new SubmissionEntity
        {
            EstablishmentId = establishmentId,
            SectionId = sectionId,
            SectionName = sectionName,
            Status = SubmissionStatus.InProgress,
            DateCreated = DateTime.UtcNow,
            CreatedUserActionId = userActionId,
            LastUpdatedUserActionId = userActionId,
        };

        await _db.Submissions.AddAsync(submission);
        await _db.SaveChangesAsync();

        return submission.Id;
    }

    private async Task<int?> GetCurrentSubmissionIdAsync(string sectionId, int establishmentId)
    {
        return await _db.Submissions
            .Where(s =>
                s.SectionId == sectionId &&
                s.EstablishmentId == establishmentId &&
                s.Status == SubmissionStatus.InProgress)
            .OrderByDescending(s => s.Id)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<int> GetOrCreateAnswerIdAsync(
        string answerContentfulId,
        string answerText)
    {
        var answerId = await _db.Answers
            .Where(a =>
                a.AnswerText == answerText &&
                a.ContentfulRef == answerContentfulId)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();

        if (answerId.HasValue)
        {
            return answerId.Value;
        }

        var answer = new AnswerEntity
        {
            AnswerText = answerText,
            ContentfulRef = answerContentfulId,
        };

        await _db.Answers.AddAsync(answer);
        await _db.SaveChangesAsync();

        return answer.Id;
    }

    private async Task<int> GetOrCreateQuestionIdAsync(
        string questionContentfulId,
        string questionText)
    {
        var questionId = await _db.Questions
            .Where(q =>
                q.QuestionText == questionText &&
                q.ContentfulRef == questionContentfulId)
            .Select(q => (int?)q.Id)
            .FirstOrDefaultAsync();

        if (questionId.HasValue)
        {
            return questionId.Value;
        }

        var question = new QuestionEntity
        {
            QuestionText = questionText,
            ContentfulRef = questionContentfulId,
        };

        await _db.Questions.AddAsync(question);
        await _db.SaveChangesAsync();

        return question.Id;
    }

    public async Task<List<SubmissionEntity>> GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(
        IEnumerable<int> establishmentIds)
    {
        var establishmentIdList = establishmentIds
            .Distinct()
            .ToList();

        var results = await dbContext.Submissions
            .Where(s =>
                establishmentIdList.Contains(s.EstablishmentId) &&
                s.Status == SubmissionStatus.CompleteReviewed &&
                !s.Deleted &&
                s.DateCompleted != null)
            .Where(s => !dbContext.Submissions.Any(s2 =>
                s2.EstablishmentId == s.EstablishmentId &&
                s2.SectionId == s.SectionId &&
                s2.SectionId == s.SectionId &&
                s2.Status == SubmissionStatus.CompleteReviewed &&
                !s2.Deleted &&
                s2.DateCompleted != null &&
                s2.DateCompleted > s.DateCompleted))
            .OrderBy(s => s.EstablishmentId)
            .ThenBy(s => s.SectionName)
            .ToListAsync();

        return results;
    }

    private RecommendationEntity BuildRecommendationEntity(SqlRecommendationDto recommendationDto)
    {
        return new RecommendationEntity
        {
            ContentfulRef = recommendationDto.ContentfulSysId,
            RecommendationText = recommendationDto.RecommendationText,
            QuestionId = recommendationDto.QuestionId,
            QuestionContentfulRef = recommendationDto.QuestionContentfulRef,
        };
    }
}
