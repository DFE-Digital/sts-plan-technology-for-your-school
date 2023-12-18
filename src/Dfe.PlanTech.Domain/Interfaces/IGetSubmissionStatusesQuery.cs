using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Interfaces;

public interface IGetSubmissionStatusesQuery
{
    IList<SectionStatusDto> GetSectionSubmissionStatuses(IEnumerable<ISectionComponent> sections);

    Task<SectionStatusNew> GetSectionSubmissionStatusAsync(int establishmentId,
                                                           ISectionComponent section,
                                                           CancellationToken cancellationToken);
}

