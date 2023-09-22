using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Helper;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Application.Submissions.Queries
{
    public class GetSubmissionStatusesQuery : IGetSubmissionStatusesQuery
    {
        private readonly IPlanTechDbContext _db;
        private readonly IUser _userHelper;

        public GetSubmissionStatusesQuery(IPlanTechDbContext db, IUser userHelper)
        {
            _db = db;
            _userHelper = userHelper;

        }

        public IList<SectionStatuses> GetSectionSubmissionStatuses(ISection[] sections)
        {
            int establishmentId = _userHelper.GetEstablishmentId().Result;
            
            string sectionStringify = string.Empty;
            sectionStringify = string.Join(',', sections.Select(x => x.Sys.Id).ToList());

            return _db.GetSectionStatuses(sectionStringify, establishmentId).ToList();
        }
    }
}
