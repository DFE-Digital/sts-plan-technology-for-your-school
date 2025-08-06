using System.Linq.Expressions;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class SubmissionRepository(PlanTechDbContext dbContext)
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

    public async Task<SubmissionEntity?> GetLatestSubmissionAsync(int establishmentId, string sectionId, bool isCompleted, bool includeRelationships = false)
    {
        var submission = await GetPreviousSubmissionsInDescendingOrder(establishmentId, sectionId, isCompleted, includeRelationships)
            .FirstOrDefaultAsync();

        return submission;
    }

    public async Task<SubmissionEntity?> GetLatestSubmissionAndResponsesAsync(int establishmentId, string sectionId, bool isCompleted)
    {
        // Get latest submission
        var submission = await GetLatestSubmissionAsync(establishmentId, sectionId, isCompleted, true);
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

    public IQueryable<SubmissionEntity> GetPreviousSubmissionsInDescendingOrder(
        int establishmentId,
        string sectionId,
        bool isCompleted,
        bool includeResponses = false
    )
    {
        List<string> statusFilters = [];
        if (isCompleted)
        {
            statusFilters = [
                SubmissionStatus.CompleteReviewed.ToString()
            ];
        }
        else
        {
            statusFilters = [
                SubmissionStatus.InProgress.ToString(),
                SubmissionStatus.CompleteNotReviewed.ToString()
            ];
        }

        return GetSubmissionsBy(submission =>
                !submission.Deleted &&
                submission.EstablishmentId == establishmentId &&
                submission.SectionId == sectionId &&
                statusFilters.Contains(submission.Status!),
                includeResponses)
            .OrderByDescending(submission => submission.DateCreated);
    }

    public Task<SubmissionEntity?> GetSubmissionByIdAsync(int submissionId, bool includeRelationships = false)
    {
        return GetSubmissionsBy(s => s.Id == submissionId).FirstOrDefaultAsync();
    }

    public IQueryable<SubmissionEntity> GetSubmissionsBy(Expression<Func<SubmissionEntity, bool>> predicate, bool includeRelationships = false)
    {
        var query = _db.Submissions
            .Where(predicate)
            .Include(s => s.Establishment);

        return includeRelationships
          ? IncludeResponses(query)
          : query;
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

    public async Task SetSubmissionInaccessibleAsync(
        int establishmentId,
        string sectionId,
        SubmissionStatus? submissionStatus = null
    )
    {
        var query = GetSubmissionsBy(submission =>
                submission.EstablishmentId == establishmentId &&
                string.Equals(submission.SectionId, sectionId)
            );

        if (submissionStatus is not null)
        {
            query = query
                .Where(s => string.Equals(s.Status, submissionStatus.ToString()));
        }

        var submission = await query.FirstOrDefaultAsync();
        if (submission is null)
        {
            return;
        }

        await SetSubmissionInaccessibleAsync(submission.Id);
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
                string.Equals(submission.SectionId, submission.SectionId) &&
                string.Equals(submission.Status, SubmissionStatus.CompleteReviewed.ToString())
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

    public async Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId)
    {
        var currentSubmission = await GetLatestSubmissionAsync(
                establishmentId,
                sectionId,
                isCompleted: true,
                includeRelationships: false
            );

        if (currentSubmission is not null)
        {
            currentSubmission.Viewed = true;
        }

        await _db.SaveChangesAsync();
    }

    public Task DeleteCurrentSubmission(int establishmentId, int sectionId)
    {
        return _db.Database.ExecuteSqlAsync(
            $@"EXEC DeleteCurrentSubmission
            @establishmentId={establishmentId},
            @sectionId={sectionId}"
        );
    }

    private IQueryable<SubmissionEntity> IncludeResponses(IQueryable<SubmissionEntity> query)
    {
        return query
            .Include(s => s.Responses)
                .ThenInclude(r => r.Question)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Answer);
    }
}
