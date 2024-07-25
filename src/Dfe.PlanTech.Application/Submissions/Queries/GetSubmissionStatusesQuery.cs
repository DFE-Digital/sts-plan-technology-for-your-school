using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;

namespace Dfe.PlanTech.Application.Submissions.Queries;

public class GetSubmissionStatusesQuery(IPlanTechDbContext db, IUser userHelper) : IGetSubmissionStatusesQuery
{

    public async Task<List<SectionStatusDto>> GetSectionSubmissionStatuses(string categoryId)
    {
        int establishmentId = await userHelper.GetEstablishmentId();

        return await db.ToListAsync(db.GetSectionStatuses(categoryId, establishmentId));
    }

    public async Task<SectionStatusNew> GetSectionSubmissionStatusAsync(int establishmentId,
                                                                         ISectionComponent section,
                                                                         bool completed,
                                                                         CancellationToken cancellationToken)
    {
        var sectionStatus = db.GetSubmissions.Where(submission => submission.EstablishmentId == establishmentId &&
                                                 submission.SectionId == section.Sys.Id && !submission.Deleted);

        var groupedAndLatest = GetLatestSubmissionStatus(sectionStatus, completed);

        var result = await db.FirstOrDefaultAsync(groupedAndLatest, cancellationToken);

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
    => submissionStatuses
        .Where(submission => completed == submission.Completed)
        .Select(submission => new SectionStatusNew()
        {
            DateCreated = submission.DateCreated,
            Completed = submission.Completed,
            Maturity = submission.Maturity,
            SectionId = submission.SectionId,
            Status = submission.Completed ? Status.Completed : submission.Responses.Count != 0 ? Status.InProgress : Status.NotStarted,
        })
        .GroupBy(submission => submission.SectionId)
        .Select(grouping => grouping.OrderByDescending(status => status.DateCreated).First());
}