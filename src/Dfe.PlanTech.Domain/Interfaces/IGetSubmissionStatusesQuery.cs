using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Interfaces;

public interface IGetSubmissionStatusesQuery
{
    IList<SectionStatusDto> GetSectionSubmissionStatuses(ISection[] sections);

    Task<SectionStatusNew?> GetSectionSubmissionStatusAsync(int establishmentId,
                                                           ISection section,
                                                           CancellationToken cancellationToken);
}

