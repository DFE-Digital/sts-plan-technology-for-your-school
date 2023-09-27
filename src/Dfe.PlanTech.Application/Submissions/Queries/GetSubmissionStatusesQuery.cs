using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

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

    public IList<SectionStatus> GetSectionSubmissionStatuses(ISection[] sections)
    {
        int establishmentId = _userHelper.GetEstablishmentId().Result;

        string sectionStringify = string.Join(',', sections.Select(x => x.Sys.Id));

        return _db.GetSectionStatuses(sectionStringify, establishmentId).ToList();
    }

    public Task<List<SectionStatusNew>> GetSectionSubmissionStatusesAsync(int establishmentId,
                                                                          IEnumerable<ISection> sections,
                                                                          CancellationToken cancellationToken)
    {
        var submissionsForSections = _db.GetSubmissions.Where(submission => submission.EstablishmentId == establishmentId &&
                                               sections.Any(section => section.Sys.Id == submission.SectionId));

        var latestSubmissionPerSection = GetLatestSubmissionStatus(submissionsForSections);

        return _db.ToListAsync(latestSubmissionPerSection, cancellationToken);
    }

    public async Task<SectionStatusNew?> GetSectionSubmissionStatusAsync(int establishmentId,
                                                                         ISection section,
                                                                         CancellationToken cancellationToken)
    {
        var sectionStatus = _db.GetSubmissions.Where(submission => submission.EstablishmentId == establishmentId &&
                                                 submission.SectionId == section.Sys.Id);

        var groupedAndLatest = GetLatestSubmissionStatus(sectionStatus);

        var result = await _db.FirstOrDefaultAsync(groupedAndLatest, cancellationToken);

        if(result != null){
            return result;
        }

        return new SectionStatusNew()
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
                                        submission.Responses.Any() ? Status.InProgress : Status.NotStarted,
    })
    .GroupBy(submission => submission.SectionId)
    .Select(grouping => grouping.OrderByDescending(status => status.DateCreated).First());
}
