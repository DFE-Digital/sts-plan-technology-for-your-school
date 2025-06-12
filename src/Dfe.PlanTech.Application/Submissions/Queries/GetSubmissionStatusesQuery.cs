using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;

namespace Dfe.PlanTech.Application.Submissions.Queries;

public class GetSubmissionStatusesQuery(IPlanTechDbContext db, IUser userHelper) : IGetSubmissionStatusesQuery
{

    public async Task<List<SectionStatusDto>> GetSectionSubmissionStatuses(IEnumerable<Section> sections, int? establishmentId = null)
    {
        if (establishmentId is null)
        {
            establishmentId = await userHelper.GetEstablishmentId();
        }

        var sectionIds = String.Join(',', sections.Select(section => section.Sys.Id));

        return await db.ToListAsync(db.GetSectionStatuses(sectionIds, establishmentId.Value));
    }

    public async Task<SectionStatus> GetSectionSubmissionStatusAsync(
        int establishmentId,
        ISectionComponent section,
        bool completed,
        CancellationToken cancellationToken)
    {
        var sectionStatus = db.GetSubmissions
            .Where(submission =>
                submission.EstablishmentId == establishmentId &&
                submission.SectionId == section.Sys.Id &&
                !submission.Deleted &&
                submission.Completed == completed);

        var ordered = sectionStatus.OrderByDescending(x => x.DateCreated);

        var latestSubmission = await db.FirstOrDefaultAsync(ordered, cancellationToken);

        if (latestSubmission != null)
        {
            return new SectionStatus
            {
                Maturity = latestSubmission.Maturity,
                SectionId = latestSubmission.SectionId,
                Completed = latestSubmission.Completed,
                Status = latestSubmission.Completed ? Status.CompleteReviewed : Status.InProgress
            };
        }

        return new SectionStatus
        {
            SectionId = section.Sys.Id,
            Completed = false,
            Status = Status.NotStarted
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

    private static Status GetStatus(Submission submission)
    {
        if (submission.Deleted)
            return Status.Inaccessible;

        if (!submission.Completed)
        {
            return submission.Responses.Any()
                ? Status.InProgress
                : Status.NotStarted;
        }

        // Completed is true, now check if it's been reviewed
        if (submission.DateCompleted != null)
            return Status.CompleteReviewed;

        return Status.CompleteNotReviewed;
    }
}
