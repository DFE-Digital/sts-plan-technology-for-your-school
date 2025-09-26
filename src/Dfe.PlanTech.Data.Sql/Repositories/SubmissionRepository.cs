using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class SubmissionRepository(PlanTechDbContext dbContext) : ISubmissionRepository
{
    protected readonly PlanTechDbContext _db = dbContext;

    public async Task<SubmissionEntity> CloneSubmission(SubmissionEntity? existingSubmission)
    {
        ArgumentNullException.ThrowIfNull(existingSubmission, nameof(existingSubmission));

        var newSubmission = new SubmissionEntity
        {
            SectionId = existingSubmission.SectionId,
            SectionName = existingSubmission.SectionName,
            EstablishmentId = existingSubmission.EstablishmentId,
            Completed = false,
            Maturity = existingSubmission.Maturity,
            DateCreated = DateTime.UtcNow,
            Status = SubmissionStatus.InProgress.ToString(),
            Responses = existingSubmission.Responses.Select(r => new ResponseEntity
            {
                QuestionId = r.QuestionId,
                AnswerId = r.AnswerId,
                UserId = r.UserId,
                Maturity = r.Maturity,
                Question = r.Question,
                Answer = r.Answer,
                DateCreated = DateTime.UtcNow
            }).ToList()
        };

        await _db.Submissions.AddAsync(newSubmission);
        await _db.SaveChangesAsync();

        return newSubmission;
    }

    public async Task ConfirmCheckAnswersAndUpdateRecommendationsAsync(int establishmentId, int? matEstablishmentId, int submissionId, int userId, QuestionnaireSectionEntry section)
    {
        var submission = _db.Submissions.FirstOrDefault(s => s.Id == submissionId);
        if (submission is null)
        {
            throw new InvalidOperationException($"Could not find submssion with ID {submissionId} in database");
        }

        var recommendationDtos = section.CoreRecommendations
            .Select(r => new SqlRecommendationDto
            {
                RecommendationText = r.Header,
                ContentfulSysId = r.Id,
                QuestionContentfulRef = r.Question.Id
            });

        var recommendations = await UpsertRecommendations(recommendationDtos);

        var responses = submission.Responses
            .Select(r => r.Answer.ContentfulRef)
            .ToHashSet();

        var answerStatusDictionary = section.CoreRecommendations
            .Select(r =>
            {
                if (r.CompletingAnswers.Any(ca => responses.Contains(ca.Id)))
                {
                    return new { r.Id, Status = RecommendationConstants.Completed };
                }

                if (r.InProgressAnswers.Any(ca => responses.Contains(ca.Id)))
                {
                    return new { r.Id, Status = RecommendationConstants.InProgress };
                }

                return new { r.Id, Status = RecommendationConstants.NotStarted };
            })
            .Where(x => x is not null)
            .ToDictionary(x => x!.Id, x => x.Status);

        var previousStatuses = _db.EstablishmentRecommendationHistories
            .Where(erh => erh.EstablishmentId == establishmentId &&
                          erh.MatEstablishmentId == matEstablishmentId)
            .ToDictionary(r => r.RecommendationId, r => r.NewStatus);

        var recommendationStatuses = recommendations.Select(r => new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishmentId,
            MatEstablishmentId = matEstablishmentId,
            RecommendationId = r.Id,
            UserId = userId,
            PreviousStatus = previousStatuses.TryGetValue(r.Id, out var previousStatus) ? previousStatus : null,
            NewStatus = answerStatusDictionary[r.ContentfulSysId]
        });

        await _db.EstablishmentRecommendationHistories.AddRangeAsync(recommendationStatuses);

        await SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(submissionId);

        await _db.SaveChangesAsync();
    }

    public async Task<SubmissionEntity?> GetLatestSubmissionAndResponsesAsync(int establishmentId, string sectionId, bool? isCompletedSubmission)
    {
        // Get latest submission
        var submission = await GetPreviousSubmissionsInDescendingOrder(establishmentId, sectionId, isCompletedSubmission)
            .FirstOrDefaultAsync();
        if (submission is null)
        {
            return null;
        }

        submission.Responses = submission.Responses
            .OrderByDescending(response => response.DateCreated)
            .GroupBy(response => response.QuestionId)
            .Select(group => group
                .OrderByDescending(response => response.DateCreated)
                .First()
            )
            .ToList();

        return submission;
    }

    public Task<SubmissionEntity?> GetSubmissionByIdAsync(int submissionId)
    {
        return GetSubmissionsBy(s => s.Id == submissionId).FirstOrDefaultAsync();
    }

    public async Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId)
    {
        var currentSubmission = await GetLatestSubmissionAndResponsesAsync(
                establishmentId,
                sectionId,
                isCompletedSubmission: true
            );

        if (currentSubmission is not null)
        {
            currentSubmission.Viewed = true;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<SubmissionEntity> SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(int submissionId)
    {
        var submission = await GetSubmissionByIdAsync(submissionId);
        if (submission is null)
        {
            throw new InvalidOperationException($"Submission not found for ID '{submissionId}'");
        }

        submission.Status = SubmissionStatus.CompleteReviewed.ToString();
        submission.DateCompleted = DateTime.UtcNow;

        var otherSubmissions = await _db.Submissions
            .Where(s =>
                s.Id != submission.Id &&
                s.EstablishmentId == submission.EstablishmentId &&
                string.Equals(s.SectionId, submission.SectionId) &&
                string.Equals(s.Status, SubmissionStatus.CompleteReviewed.ToString())
            )
            .ToListAsync();

        foreach (var oldSubmissions in otherSubmissions)
        {
            oldSubmissions.Status = SubmissionStatus.Inaccessible.ToString();
            oldSubmissions.Deleted = true;
        }

        await _db.SaveChangesAsync();

        return submission;
    }

    public async Task SetSubmissionInaccessibleAsync(
        int establishmentId,
        string sectionId
    )
    {
        var query = GetSubmissionsBy(submission =>
                submission.EstablishmentId == establishmentId &&
                submission.SectionId == sectionId
            );

        var submission = await query.FirstOrDefaultAsync();
        if (submission is null)
        {
            throw new InvalidOperationException($"Submission not found for establishment ID '{establishmentId}' and section ID '{sectionId}'");
        }

        await SetSubmissionInaccessibleAsync(submission.Id);
    }

    public async Task<SubmissionEntity> SetSubmissionInaccessibleAsync(int submissionId)
    {
        var submission = await GetSubmissionByIdAsync(submissionId);
        if (submission is null)
        {
            throw new InvalidOperationException($"Submission not found for ID '{submissionId}'");
        }

        submission.Status = SubmissionStatus.Inaccessible.ToString();
        await _db.SaveChangesAsync();

        return submission;
    }

    private IQueryable<SubmissionEntity> GetPreviousSubmissionsInDescendingOrder(
        int establishmentId,
        string sectionId,
        bool? isCompletedSubmission
    )
    {
        return GetSubmissionsBy(submission =>
                !submission.Deleted &&
                submission.EstablishmentId == establishmentId &&
                submission.SectionId == sectionId &&
                (isCompletedSubmission == null || submission.Completed == isCompletedSubmission)
            )
            .OrderByDescending(submission => submission.DateCreated);
    }

    private IQueryable<SubmissionEntity> GetSubmissionsBy(Expression<Func<SubmissionEntity, bool>> predicate)
    {
        var query = _db.Submissions
            .Where(predicate)
            .Include(s => s.Establishment)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Question)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Answer);

        return query;
    }

    private async Task<IEnumerable<SqlRecommendationDto>> UpsertRecommendations(IEnumerable<SqlRecommendationDto> recommendationDtos)
    {
        var contentfulRefs = recommendationDtos.Select(cr => cr.ContentfulSysId);
        var existingRecommendations = await _db.Recommendations
            .Where(recommendation => contentfulRefs.Contains(recommendation.ContentfulRef))
            .Where(recommendation => recommendation != null)
            .ToListAsync();

        var existingRecommendationContentfulRefs = existingRecommendations.Select(r => r.ContentfulRef).ToList();

        var recommendationsToInsert = recommendationDtos
            .Where(rm => !existingRecommendationContentfulRefs.Contains(rm.ContentfulSysId))
            .Select(BuildRecommendationEntity)
            .ToList();

        _db.AddRange(recommendationsToInsert);

        var recommendationDtoDictionary = recommendationDtos.ToDictionary(r => r.ContentfulSysId, r => r);
        var recommendationsToUpdate = new List<SqlRecommendationDto>();

        foreach (var existingRecommendation in existingRecommendations)
        {
            recommendationDtoDictionary.TryGetValue(existingRecommendation.ContentfulRef, out var recommendationDto);
            if (recommendationDto is null)
            {
                continue;
            }

            if (!string.Equals(recommendationDto.RecommendationText, existingRecommendation.RecommendationText))
            {
                recommendationsToUpdate.Add(recommendationDto);
            }
        }

        if (recommendationsToUpdate.Any())
        {
            var recommendationEntitiesToUpdate = recommendationsToUpdate.Select(BuildRecommendationEntity);
            _db.Recommendations.UpdateRange(recommendationEntitiesToUpdate);
        }

        return await _db.Recommendations.Where(r => contentfulRefs.Contains(r.ContentfulRef)).ToListAsync();
    }

    private RecommendationEntity BuildRecommendationEntity(SqlRecommendationDto recommendationDto)
    {
        return new RecommendationEntity
        {
            ContentfulRef = recommendationDto.ContentfulSysId,
            RecommendationText = recommendationDto.RecommendationText,
            QuestionContentfulRef = recommendationDto.QuestionContentfulRef
        };
    }
}
