using System.Linq.Expressions;
using Dfe.PlanTech.Core.Constants;
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
        var submission = await _db.Submissions
            .Include(s => s.Responses)
                .ThenInclude(r => r.Answer)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission is null)
        {
            throw new InvalidOperationException($"Could not find submssion with ID {submissionId} in database");
        }

        var answeredQuestionIds = submission.Responses.Select(response => response.QuestionId);

        var questions = await _db.Questions
            .Where(q => answeredQuestionIds.Contains(q.Id))
            .ToListAsync();

        var recommendationDtos = section.CoreRecommendations
            .Select(r =>
            {
                var question = questions
                    .Where(q => string.Equals(q.ContentfulRef, r.Question.Id))
                    .FirstOrDefault();

                if (question is null)
                {
                    throw new InvalidOperationException("Could not find the question identified in the submission.");
                }

                return new SqlRecommendationDto
                {
                    RecommendationText = r.Header,
                    ContentfulSysId = r.Id,
                    QuestionId = question.Id
                };
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
                    return new { r.Id, Status = RecommendationConstants.CompletedKey };
                }

                if (r.InProgressAnswers.Any(ca => responses.Contains(ca.Id)))
                {
                    return new { r.Id, Status = RecommendationConstants.InProgressKey };
                }

                return new { r.Id, Status = RecommendationConstants.NotStartedKey };
            })
            .Where(x => x is not null)
            .ToDictionary(x => x!.Id, x => x.Status);

        var previousStatuses = _db.EstablishmentRecommendationHistories
            .Where(erh => erh.EstablishmentId == establishmentId &&
                          erh.MatEstablishmentId == matEstablishmentId)
            .GroupBy(erh => erh.RecommendationId, erh => erh)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(erh => erh.DateCreated).First().NewStatus);

        var recommendationStatuses = recommendations.Select(r => new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishmentId,
            MatEstablishmentId = matEstablishmentId,
            RecommendationId = r.Id,
            UserId = userId,
            PreviousStatus = previousStatuses.TryGetValue(r.Id, out var previousStatus) ? previousStatus : null,
            NewStatus = answerStatusDictionary[r.ContentfulRef]
        });

        await _db.EstablishmentRecommendationHistories.AddRangeAsync(recommendationStatuses);

        await SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(submissionId);

        // No need to save changes as this is done in the call above
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

        submission.Completed = true;
        submission.DateCompleted = DateTime.UtcNow;
        submission.Status = SubmissionStatus.CompleteReviewed.ToString();

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

    private async Task<List<RecommendationEntity>> UpsertRecommendations(IEnumerable<SqlRecommendationDto> recommendationDtos)
    {
        var contentfulRefs = recommendationDtos.Select(r => r.ContentfulSysId);
        var existingRecommendations = await _db.Recommendations
            .Where(recommendation => contentfulRefs.Contains(recommendation.ContentfulRef))
            .Where(recommendation => recommendation != null)
            .ToListAsync();

        var existingRecommendationContentfulRefs = existingRecommendations.Select(r => r.ContentfulRef).ToList();

        var recommendationEntitiesToInsert = recommendationDtos
            .Where(rm => !existingRecommendationContentfulRefs.Contains(rm.ContentfulSysId))
            .Select(BuildRecommendationEntity)
            .ToList();


        var recommendationDtoDictionary = recommendationDtos.ToDictionary(r => r.ContentfulSysId, r => r);
        var recommendationsWithNoChanges = new List<RecommendationEntity>();

        foreach (var existingRecommendation in existingRecommendations)
        {
            recommendationDtoDictionary.TryGetValue(existingRecommendation.ContentfulRef, out var recommendationDto);
            if (recommendationDto is null)
            {
                continue;
            }

            if (!string.Equals(recommendationDto.RecommendationText, existingRecommendation.RecommendationText))
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

        return await _db.Recommendations.Where(r => contentfulRefs.Contains(r.ContentfulRef)).ToListAsync();
    }

    private RecommendationEntity BuildRecommendationEntity(SqlRecommendationDto recommendationDto)
    {
        return new RecommendationEntity
        {
            ContentfulRef = recommendationDto.ContentfulSysId,
            RecommendationText = recommendationDto.RecommendationText,
            QuestionId = recommendationDto.QuestionId
        };
    }
}
