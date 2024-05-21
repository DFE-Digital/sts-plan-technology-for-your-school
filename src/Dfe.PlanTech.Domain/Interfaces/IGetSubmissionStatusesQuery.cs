using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Interfaces;

public interface IGetSubmissionStatusesQuery
{
    Task<List<SectionStatusDto>> GetSectionSubmissionStatuses(string categoryId);

    Task<SectionStatusNew> GetSectionSubmissionStatusAsync(int establishmentId,
                                                           ISectionComponent section,
                                                           CancellationToken cancellationToken);
}

