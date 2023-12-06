using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Interfaces;

public interface IGetSubmissionStatusesQuery
{
    IList<SectionStatusDto> GetSectionSubmissionStatuses(IEnumerable<ISectionContentComponent> sections);

    Task<SectionStatusNew> GetSectionSubmissionStatusAsync(int establishmentId,
                                                           ISectionContentComponent section,
                                                           CancellationToken cancellationToken);
}

