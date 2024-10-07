using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

public interface IGetSubmissionStatusesQuery
{
    Task<List<SectionStatusDto>> GetSectionSubmissionStatuses(string categoryId);

    Task<SectionStatus> GetSectionSubmissionStatusAsync(int establishmentId,
                                                           ISectionComponent section,
                                                           bool completed,
                                                           CancellationToken cancellationToken);
}

