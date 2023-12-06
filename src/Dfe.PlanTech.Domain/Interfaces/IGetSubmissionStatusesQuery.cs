using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Interfaces;

public interface IGetSubmissionStatusesQuery
{
    IList<SectionStatusDto> GetSectionSubmissionStatuses(IEnumerable<Section> sections);

    Task<SectionStatusNew> GetSectionSubmissionStatusAsync(int establishmentId,
                                                           Section section,
                                                           CancellationToken cancellationToken);
}

