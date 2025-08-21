using System.Linq.Expressions;
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

    public Task DeleteCurrentSubmission(int establishmentId, int sectionId)
    {
        return _db.Database.ExecuteSqlAsync(
            $@"EXEC DeleteCurrentSubmission
            @establishmentId={establishmentId},
            @sectionId={sectionId}"
        );
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
}
