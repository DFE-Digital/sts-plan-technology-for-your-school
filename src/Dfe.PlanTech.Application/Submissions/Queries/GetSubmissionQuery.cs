using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Interface;

namespace Dfe.PlanTech.Application.Submissions.Queries;

public class GetSubmissionQuery : IGetSubmissionQuery
{
    private readonly IPlanTechDbContext _db;

    public GetSubmissionQuery(IPlanTechDbContext db)
    {
        _db = db;
    }
    public async Task<Domain.Submissions.Models.Submission?> GetSubmissionBy(int establishmentId, string sectionId)
    {
        var submissions = _db.GetSubmissions
                                        .Where(submission => submission.EstablishmentId == establishmentId && submission.SectionId.Equals(sectionId))
                                        .OrderByDescending(submission => submission.DateCreated);

        return await _db.FirstOrDefaultAsync(submissions);
    }
}