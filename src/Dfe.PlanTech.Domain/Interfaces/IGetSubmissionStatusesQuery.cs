using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Interfaces
{
    public interface IGetSubmissionStatusesQuery
    {
        Task<IList<SectionStatuses>> GetSectionSubmissionStatuses(IEnumerable<ISection> sections);
    }
}
