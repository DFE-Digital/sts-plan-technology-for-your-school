using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

public interface IGetSubmissionStatusesQuery
{
    Task<List<SectionStatusDto>> GetSectionSubmissionStatuses(IEnumerable<Section> sections);

    Task<SectionStatus> GetSectionSubmissionStatusAsync(int establishmentId,
                                                           ISectionComponent section,
                                                           bool completed,
                                                           CancellationToken cancellationToken);
}

