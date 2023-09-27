using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
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

    public IList<SectionStatus> GetSectionSubmissionStatuses(ISection[] sections)
    {
        int establishmentId = _userHelper.GetEstablishmentId().Result;

        string sectionStringify = string.Empty;
        sectionStringify = string.Join(',', sections.Select(x => x.Sys.Id).ToList());

        return _db.GetSectionStatuses(sectionStringify, establishmentId).ToList();
    }

    public Task<List<SectionStatus>> GetSectionSubmissionStatusesAsync(int establishmentId,
                                                                       IEnumerable<ISection> sections,
                                                                       CancellationToken cancellationToken)
    {
        var submissionsForSections = _db.GetSubmissions.Where(submission => submission.EstablishmentId == establishmentId &&
                                               sections.Any(section => section.Sys.Id == submission.SectionId));

        var latestSubmissionPerSection = GetLatestSubmissionStatus(submissionsForSections);

        return _db.ToListAsync(latestSubmissionPerSection, cancellationToken);
    }

    public Task<SectionStatusWithLatestResponse?> GetSectionSubmissionStatusAsync(int establishmentId,
                                                               ISection section,
                                                               CancellationToken cancellationToken)
    {
        var sectionStatus = _db.GetSubmissions.Where(submission => submission.EstablishmentId == establishmentId &&
                                                 submission.SectionId == section.Sys.Id)
                           .Select(submission => new
                           {
                               LatestResponse = submission.Responses
                                                             .OrderByDescending(response => response.DateCreated)
                                                             .Select(response => new SectionResponseDto(
                                                                response.Question.ContentfulRef,
                                                                response.Answer.ContentfulRef
                                                             ))
                                                             .FirstOrDefault(),
                               SectionStatus = new SectionStatus()
                               {
                                   DateCreated = submission.DateCreated,
                                   Completed = submission.Completed,
                                   Maturity = submission.Maturity,
                                   SectionId = submission.SectionId
                               }
                           })
                           .GroupBy(submission => submission.SectionStatus.SectionId)
                           .Select(grouping => grouping.OrderByDescending(group => group.SectionStatus.DateCreated)
                                                       .First())
                            .Select(latest => new SectionStatusWithLatestResponse(latest.SectionStatus, latest.LatestResponse));

        return _db.FirstOrDefaultAsync(sectionStatus, cancellationToken);
    }

    /// <summary>
    /// For each submission, convert to SectionStatus, 
    /// group by Section, 
    /// then return latest for each grouping
    /// </summary>
    /// <param name="submissionStatuses"></param>
    /// <returns></returns>
    private static IQueryable<SectionStatus> GetLatestSubmissionStatus(IQueryable<Submission> submissionStatuses)
    => submissionStatuses.Select(submission => new SectionStatus()
    {
        DateCreated = submission.DateCreated,
        Completed = submission.Completed,
        Maturity = submission.Maturity,
        SectionId = submission.SectionId
    })
    .GroupBy(submission => submission.SectionId)
    .Select(grouping => grouping.OrderByDescending(status => status.DateCreated).First());
}
