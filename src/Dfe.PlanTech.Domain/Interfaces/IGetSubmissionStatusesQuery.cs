using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Interfaces
{
    public interface IGetSubmissionStatusesQuery
    {
        public ICategory GetCategoryWithCompletedSectionStatuses(ICategory category);

        IList<SectionStatuses> GetSectionSubmissionStatuses(ISection[] sections);
    }
}
