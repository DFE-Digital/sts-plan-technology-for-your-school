using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;

namespace Dfe.PlanTech.Application.Submissions.Queries;

public class GetSubmissionStatusesQuery : IGetSubmissionStatusesQuery
{
    private readonly IPlanTechDbContext _db;
    private readonly IUser _userHelper;

    public GetSubmissionStatusesQuery(IPlanTechDbContext db, IUser userHelper)
    {
        _db = db;
        _userHelper = userHelper;

    }

    public async Task<List<SectionStatusDto>> GetSectionSubmissionStatuses(string categoryId)
    {
        int establishmentId = await _userHelper.GetEstablishmentId();

        return await _db.ToListAsync(_db.GetSectionStatuses(categoryId, establishmentId));
    }

    public async Task<SectionStatusNew> GetSectionSubmissionStatusAsync(int establishmentId,
                                                                         ISectionComponent section,
                                                                         bool completed,
                                                                         CancellationToken cancellationToken)
    {
        var sectionStatus = _db.GetSubmissions.Where(submission => submission.EstablishmentId == establishmentId &&
                                                 submission.SectionId == section.Sys.Id && !submission.Deleted);

        var groupedAndLatest = GetLatestSubmissionStatus(sectionStatus, completed);

        var result = await _db.FirstOrDefaultAsync(groupedAndLatest, cancellationToken);

        return result ?? new SectionStatusNew()
        {
            SectionId = section.Sys.Id,
            Completed = false,
            Status = Status.NotStarted,
        };
    }

    /// <summary>
    /// For each submission, convert to SectionStatus, 
    /// group by Section, 
    /// then return latest for each grouping
    /// optionally choosing from completed submissions first
    /// </summary>
    /// <param name="submissionStatuses"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    private static IQueryable<SectionStatusNew> GetLatestSubmissionStatus(IQueryable<Submission> submissionStatuses, bool completed)
        => submissionStatuses.Select(submission => new SectionStatusNew()
        {
            DateCreated = submission.DateCreated,
            Completed = submission.Completed,
            Maturity = submission.Maturity,
            SectionId = submission.SectionId,
            Status = GetSubmissionStatus(submission),
        })
        .GroupBy(submission => submission.SectionId)
        .Select(grouping => grouping
            .OrderByDescending(status => completed && status.Completed)
            .ThenByDescending(status => status.DateCreated)
            .First());

    private static Status GetSubmissionStatus(Submission submission) => submission.Completed ? Status.Completed : GetSubmissionStatus(submission.Responses);

    private static Status GetSubmissionStatus(List<Response>? responses) => (responses?.Count ?? 0) > 0 ? Status.InProgress : Status.NotStarted;
}