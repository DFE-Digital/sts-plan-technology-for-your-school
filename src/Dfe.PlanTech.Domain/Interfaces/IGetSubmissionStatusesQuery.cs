using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Interfaces
{
    public interface IGetSubmissionStatusesQuery
    {
        IList<SectionStatuses> GetSectionSubmissionStatuses(ISection[] sections);
    }
}
