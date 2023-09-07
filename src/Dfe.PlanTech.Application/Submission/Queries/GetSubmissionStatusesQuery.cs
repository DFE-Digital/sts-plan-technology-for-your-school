using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Application.Submission.Queries
{
    public class GetSubmissionStatusesQuery : IGetSubmissionStatusesQuery
    {
        private readonly IPlanTechDbContext _db;

        public GetSubmissionStatusesQuery(IPlanTechDbContext db)
        {
            _db = db;
        }

        public async Task<IList<SectionStatuses>> GetSectionSubmissionStatuses(IEnumerable<ISection> sections)
        {
            string sectionStringify = string.Join(',', sections.Select(x => x.Sys.Id));

            return await  _db.ToListAsync(_db.GetSectionStatuses(sectionStringify));
        }
    }
}
