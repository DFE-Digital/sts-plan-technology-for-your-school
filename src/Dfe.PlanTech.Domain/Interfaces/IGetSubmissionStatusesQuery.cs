using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Interfaces;

public interface IGetSubmissionStatusesQuery
{
    IList<SectionStatus> GetSectionSubmissionStatuses(ISection[] sections);

    Task<List<SectionStatusNew>> GetSectionSubmissionStatusesAsync(int establishmentId,
                                                                IEnumerable<ISection> sections,
                                                                CancellationToken cancellationToken);

    Task<SectionStatusNew?> GetSectionSubmissionStatusAsync(int establishmentId,
                                                           ISection section,
                                                           CancellationToken cancellationToken);
}

