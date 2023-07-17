using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;

namespace Dfe.PlanTech.Application.Submission.Queries
{
    public class GetSubmissionStatusesQuery : IGetSubmissionStatusesQuery
    {
        private readonly IPlanTechDbContext _db;

        public GetSubmissionStatusesQuery(IPlanTechDbContext db)
        {
            _db = db;
        }

        public async Task<IDictionary<string, string>> GetSectionSubmissionStatuses()
        {
            return new Dictionary<string, string> { { "3XQEHYfvEQkQwdrihDGagJ", "Completed" }, { "2gH0ajGDWUeubWuR5llzsS", "Started" } };

        }
    }
}
