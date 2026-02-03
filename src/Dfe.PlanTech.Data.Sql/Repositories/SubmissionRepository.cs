using System.Linq.Expressions;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class SubmissionRepository(PlanTechDbContext dbContext) : ISubmissionRepository
{
    protected readonly PlanTechDbContext _db = dbContext;

    public async Task<SubmissionEntity> CloneSubmission(SubmissionEntity? existingSubmission)
    {
        ArgumentNullException.ThrowIfNull(existingSubmission);

        var newSubmission = new SubmissionEntity
        {
            SectionId = existingSubmission.SectionId,
            SectionName = existingSubmission.SectionName,
            EstablishmentId = existingSubmission.EstablishmentId,
            Maturity = existingSubmission.Maturity,
            DateCreated = DateTime.UtcNow,
            Status = SubmissionStatus.InProgress,
            Responses = existingSubmission
                .Responses.Select(r => new ResponseEntity
                {
                    QuestionId = r.QuestionId,
                    AnswerId = r.AnswerId,
                    UserId = r.UserId,
                    UserEstablishmentId = r.UserEstablishmentId,
                    Maturity = r.Maturity,
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

        // Create recommendation dtos each of the core recs
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
        // No need to save changes as this is done in the call above
    }

    public async Task<SubmissionEntity?> GetLatestSubmissionAndResponsesAsync(
        int establishmentId,
        string sectionId,
        SubmissionStatus? status
    )
    {
        // Get latest submission
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

    public async Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId)
    {
        var currentSubmission = await GetLatestSubmissionAndResponsesAsync(
            establishmentId,
            sectionId,
            status: SubmissionStatus.CompleteReviewed
        );

        if (currentSubmission is not null)
        {
            currentSubmission.Viewed = true;
            await _db.SaveChangesAsync();
        }
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

        submission.DateCompleted = DateTime.UtcNow;
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
            status: SubmissionStatus.InProgress
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
        await _db.SaveChangesAsync();

        return submission;
    }

    public async Task SetSubmissionInProgressAsync(int establishmentId, string sectionId)
    {
        var query = GetPreviousSubmissionsInDescendingOrder(
            establishmentId,
            sectionId,
            status: SubmissionStatus.InProgress
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
    public async Task<List<SectionStatusEntity>> GetSectionStatusesAsync(string sectionIds, int establishmentId)
    {
        var sectionIdList = sectionIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var currentSubmissions = await _db.Submissions
            .Where(s =>
                !s.Deleted &&
                s.EstablishmentId == establishmentId &&
                sectionIdList.Contains(s.SectionId))
            .GroupBy(s => s.SectionId)
            .Select(g => g
                .OrderByDescending(s => s.DateCreated)
                .First())
            .ToListAsync();

        var lastCompleteSubmissions = await _db.Submissions
            .Where(s =>
                !s.Deleted &&
                s.EstablishmentId == establishmentId &&
                sectionIdList.Contains(s.SectionId) &&
                (s.Status == SubmissionStatus.CompleteReviewed))
            .GroupBy(s => s.SectionId)
            .Select(g => g
                .OrderByDescending(s => s.DateCreated)
                .First())
            .ToListAsync();

        var currentBySectionId = currentSubmissions.ToDictionary(s => s.SectionId, s => s);
        var lastCompleteBySectionId = lastCompleteSubmissions.ToDictionary(s => s.SectionId, s => s);

        var result = sectionIdList.Select(sectionId =>
        {
            currentBySectionId.TryGetValue(sectionId, out var currentSubmission);
            lastCompleteBySectionId.TryGetValue(sectionId, out var lastCompleteSubmission);

            return new SectionStatusEntity
            {
                SectionId = sectionId,
                Status = currentSubmission?.Status ?? SubmissionStatus.NotStarted,
                DateCreated = currentSubmission?.DateCreated ?? DateTime.UtcNow,
                DateUpdated = currentSubmission?.DateLastUpdated ?? currentSubmission?.DateCreated ?? DateTime.UtcNow,
                LastMaturity = lastCompleteSubmission?.Maturity,
                LastCompletionDate = lastCompleteSubmission?.DateCompleted,
                Viewed = lastCompleteSubmission?.Viewed
            };
        }).ToList();

        return result;
    }

    public async Task SetSubmissionDeletedAsync(int establishmentId, string sectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionId);

        var submissionId = await _db.Submissions
            .Where(s => s.SectionId == sectionId
                     && s.EstablishmentId == establishmentId
                     && s.Status != SubmissionStatus.Inaccessible)
            .OrderByDescending(s => s.Id)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();

        if (submissionId is null)
            return;

        await _db.Submissions
            .Where(s => s.Id == submissionId.Value)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(s => s.Deleted, true));
    }

    private RecommendationEntity BuildRecommendationEntity(SqlRecommendationDto recommendationDto)
    {
        return new RecommendationEntity
        {
            ContentfulRef = recommendationDto.ContentfulSysId,
            RecommendationText = recommendationDto.RecommendationText,
            QuestionId = recommendationDto.QuestionId,
        };
    }
}
