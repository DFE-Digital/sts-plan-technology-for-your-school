using System.Linq.Expressions;
using Dfe.PlanTech.Core.DataTransferObjects;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Infrastructure.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

public class SubmissionRepository
{
    protected readonly PlanTechDbContext _db;

    public SubmissionRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

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

    public Task<SubmissionEntity?> GetSubmissionByIdAsync(int submissionId, bool includeRelationships = false)
    {
        var query = _db.Submissions.Where(s => s.Id == submissionId);

        return includeRelationships
            ? IncludeRelationships(query).FirstOrDefaultAsync()
            : query.FirstOrDefaultAsync();
    }

    public IQueryable<SubmissionEntity> GetSubmissionsBy(Expression<Func<SubmissionEntity, bool>> predicate, bool includeRelationships = false)
    {
        var query = _db.Submissions.Where(predicate);

        return includeRelationships
          ? IncludeRelationships(query)
          : query;
    }

    public IQueryable<SubmissionEntity> GetPreviousSubmissions(
        int establishmentId,
        string sectionId,
        bool isCompleted,
        bool includeRelationships = false
    )
    {
        Func<string, bool> statusCheck;
        if (isCompleted)
        {
            statusCheck = (string status) => Equals(status, SubmissionStatus.CompleteReviewed);
        }
        else
        {
            statusCheck = (string status) => Equals(status, SubmissionStatus.InProgress) ||
                                             Equals(status, SubmissionStatus.CompleteNotReviewed);
        }

        return GetSubmissionsBy(submission =>
                !submission.Deleted &&
                submission.EstablishmentId == establishmentId &&
                submission.SectionId == sectionId,
                includeRelationships)
            .Where(s => statusCheck(s.Status!))
            .OrderByDescending(submission => submission.DateCreated);
    }

    public async Task<SubmissionEntity?> GetLatestSubmissionAsync(int establishmentId, string sectionId, bool isCompleted, bool includeRelationships = false)
    {
        var submission = await GetPreviousSubmissions(establishmentId, sectionId, isCompleted, includeRelationships)
            .OrderByDescending(submission => submission.DateCreated)
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

        var latestResponses = submission.Responses
            .GroupBy(response => response.QuestionId)
            .Select(group => group
                .Where(response => response.DateCreated == group.Max(r => r.DateCreated))
                .First());

        submission.Responses = latestResponses.ToList();

        return submission;
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

    public async Task SetPreviousCompletedReviewedSubmissionsInaccessible(SubmissionEntity currentSubmission)
    {
        var otherSubmissions = await GetSubmissionsBy(submission =>
                submission.EstablishmentId == currentSubmission.EstablishmentId &&
                string.Equals(submission.SectionId, currentSubmission.SectionId) &&
                string.Equals(submission.Status, SubmissionStatus.CompleteReviewed.ToString()) &&
                submission.Id != currentSubmission.Id
            )
            .ToListAsync();

        foreach (var oldSubmissions in otherSubmissions)
        {
            oldSubmissions.Status = SubmissionStatus.Inaccessible.ToString();
            oldSubmissions.Deleted = true;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<SubmissionEntity> SetSubmissionReviewedAsync(int submissionId)
    {
        var submission = await GetSubmissionByIdAsync(submissionId);
        if (submission is null)
        {
            throw new InvalidOperationException($"Submission not found for ID '{submissionId}'");
        }

        submission.Status = SubmissionStatus.CompleteReviewed.ToString();
        submission.DateCompleted = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return submission;
    }

    public async Task SetLatestSubmissionViewedAsync(int establishmentId, string sectionId)
    {
        var currentSubmission = await GetPreviousSubmissions(
                establishmentId,
                sectionId,
                (string status) => Equals(status, SubmissionStatus.CompleteReviewed),
                includeRelationships: true
            )
            .FirstOrDefaultAsync();

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

    private IQueryable<SubmissionEntity> IncludeRelationships(IQueryable<SubmissionEntity> query)
    {
        return query
            .Include(s => s.Responses)
                .ThenInclude(r => r.Question)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Answer);
    }
}
