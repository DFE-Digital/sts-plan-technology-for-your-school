using System.Linq.Expressions;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Providers.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class SubmissionRepository(
    PlanTechDbContext dbContext,
    IUserActionIdProvider userActionIdProvider
) : ISubmissionRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    private readonly IUserActionIdProvider _userActionIdProvider =
        userActionIdProvider ?? throw new ArgumentNullException(nameof(userActionIdProvider));

    public async Task<SubmissionEntity> CloneSubmission(SubmissionEntity? existingSubmission)
    {
        ArgumentNullException.ThrowIfNull(existingSubmission);

        var userActionId = _userActionIdProvider.GetUserActionId();

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

    public Task<SubmissionEntity?> GetLatestCompletedSubmissionBySectionIdAsync(
        int establishmentId,
        string sectionId
    )
    {
        return GetPreviousSubmissionsInDescendingOrder(
                establishmentId,
                sectionId,
                SubmissionStatus.CompleteReviewed
            )
            .FirstOrDefaultAsync();
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
        var submission =
            await GetSubmissionByIdAsync(submissionId)
            ?? throw new InvalidOperationException($"Submission not found for ID '{submissionId}'");
        var userActionId = _userActionIdProvider.GetUserActionId();

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
            statuses: [SubmissionStatus.InProgress, SubmissionStatus.Inaccessible]
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
        submission.LastUpdatedUserActionId = _userActionIdProvider.GetUserActionId();

        await _db.SaveChangesAsync();

        return submission;
    }

    public async Task SetSubmissionInProgressAsync(int establishmentId, string sectionId)
    {
        var query = GetPreviousSubmissionsInDescendingOrder(
            establishmentId,
            sectionId,
            statuses: [SubmissionStatus.InProgress, SubmissionStatus.Inaccessible]
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
            submission.LastUpdatedUserActionId = _userActionIdProvider.GetUserActionId();

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
            throw new ArgumentException(
                "At least one submission status must be provided",
                nameof(statuses)
            );

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

    public async Task<List<SectionStatusEntity>> GetSectionStatusesAsync(
        string sectionIds,
        int establishmentId
    )
    {
        var sectionIdList = sectionIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var currentSubmissionsRaw = await _db
            .Submissions.Where(s =>
                !s.Deleted
                && s.EstablishmentId == establishmentId
                && sectionIdList.Contains(s.SectionId)
            )
            .ToListAsync();

        var currentSubmissions = currentSubmissionsRaw
            .GroupBy(s => s.SectionId)
            .Select(g => g.OrderByDescending(s => s.DateCreated).First());

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
                    LastUpdatedUserActionId = currentSubmission?.LastUpdatedUserActionId,
                    CreatedUserActionId = currentSubmission?.CreatedUserActionId,
                    CompletedUserActionId = lastCompleteSubmission?.CompletedUserActionId,
                };
            })
            .ToList();

        return result;
    }

    public async Task UpdateSubmissionDatesAsync(int submissionId, Guid userActionId)
    {
        await _db
            .Submissions.Where(s => s.Id == submissionId)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(s => s.DateLastUpdated, DateTime.UtcNow)
                    .SetProperty(s => s.LastUpdatedUserActionId, userActionId)
            );

        await _db.SaveChangesAsync();
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

        var userActionId = _userActionIdProvider.GetUserActionId();

        await _db
            .Submissions.Where(s => s.Id == submissionId.Value)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(s => s.Deleted, true)
                    .SetProperty(s => s.DateLastUpdated, DateTime.UtcNow)
                    .SetProperty(s => s.LastUpdatedUserActionId, userActionId)
            );
    }

    public async Task<int> SelectOrInsertSubmissionAsync(
        string sectionId,
        string sectionName,
        int establishmentId
    )
    {
        var submissionId = await GetCurrentSubmissionIdAsync(sectionId, establishmentId);

        if (submissionId.HasValue)
        {
            return submissionId.Value;
        }

        var userActionId = _userActionIdProvider.GetUserActionId();

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
        return await _db
            .Submissions.Where(s =>
                s.SectionId == sectionId
                && s.EstablishmentId == establishmentId
                && s.Status == SubmissionStatus.InProgress
            )
            .OrderByDescending(s => s.Id)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<
        List<SubmissionEntity>
    > GetLatestEstablishmentsCompletedSubmissionsBySectionsAsync(IEnumerable<int> establishmentIds)
    {
        var establishmentIdList = establishmentIds.Distinct().ToList();

        var results = await _db
            .Submissions.Include(s => s.Establishment)
            .Where(s =>
                establishmentIdList.Contains(s.EstablishmentId)
                && s.Status == SubmissionStatus.CompleteReviewed
                && !s.Deleted
                && s.DateCompleted != null
            )
            .Where(s =>
                !_db.Submissions.Any(s2 =>
                    s2.EstablishmentId == s.EstablishmentId
                    && s2.SectionId == s.SectionId
                    && s2.Status == SubmissionStatus.CompleteReviewed
                    && !s2.Deleted
                    && s2.DateCompleted != null
                    && s2.DateCompleted > s.DateCompleted
                )
            )
            .OrderBy(s => s.EstablishmentId)
            .ThenBy(s => s.SectionName)
            .ToListAsync();

        return results;
    }

    public async Task<List<SubmissionEntity>> GetLatestSubmissionPerEstablishmentForSectionAsync(
        IEnumerable<int> establishmentIds,
        string sectionId
    )
    {
        var establishmentIdList = establishmentIds.Distinct().ToList();

        var results = await dbContext
            .Submissions.Where(s =>
                establishmentIdList.Contains(s.EstablishmentId)
                && s.SectionId == sectionId
                && !s.Deleted
                && s.DateLastUpdated != null
            )
            .Where(s =>
                !dbContext.Submissions.Any(s2 =>
                    s2.EstablishmentId == s.EstablishmentId
                    && s2.SectionId == s.SectionId
                    && !s2.Deleted
                    && s2.DateLastUpdated != null
                    && s2.DateLastUpdated > s.DateLastUpdated
                )
            )
            .OrderBy(s => s.EstablishmentId)
            .ThenBy(s => s.SectionName)
            .ToListAsync();

        return results;
    }
}
