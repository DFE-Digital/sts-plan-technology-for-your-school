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

    public Task<List<SectionStatusDto>> GetSectionSubmissionStatuses(IEnumerable<ISectionComponent> sections)
    {
        int establishmentId = _userHelper.GetEstablishmentId().Result;

        string sectionStringify = string.Join(',', sections.Select(x => x.Sys.Id));

        return _db.ToListAsync(_db.GetSectionStatuses(sectionStringify, establishmentId));
    }

    public async Task<SectionStatusNew> GetSectionSubmissionStatusAsync(int establishmentId,
                                                                         ISectionComponent section,
                                                                         CancellationToken cancellationToken)
    {
        var sectionStatus = _db.GetSubmissions.Where(submission => submission.EstablishmentId == establishmentId &&
                                                 submission.SectionId == section.Sys.Id && !submission.Deleted);

        var groupedAndLatest = GetLatestSubmissionStatus(sectionStatus);

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
    /// </summary>
    /// <param name="submissionStatuses"></param>
    /// <returns></returns>
    private static IQueryable<SectionStatusNew> GetLatestSubmissionStatus(IQueryable<Submission> submissionStatuses)
    => submissionStatuses.Select(submission => new SectionStatusNew()
    {
        DateCreated = submission.DateCreated,
        Completed = submission.Completed,
        Maturity = submission.Maturity,
        SectionId = submission.SectionId,
        Status = submission.Completed ? Status.Completed :
                                        submission.Responses.Count != 0 ? Status.InProgress : Status.NotStarted,
    })
    .GroupBy(submission => submission.SectionId)
    .Select(grouping => grouping.OrderByDescending(status => status.DateCreated).First());
}
