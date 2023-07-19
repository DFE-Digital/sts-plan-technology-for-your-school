using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;
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

        public IList<SectionStatuses> GetSectionSubmissionStatuses(ISection[] sections)
        {
           string sectionStringify = string.Empty;
           sectionStringify = string.Join(',', sections.Select(x => x.Sys.Id).ToList());

           return _db.GetSectionStatuses(sectionStringify).ToList();
        }
    }
}
