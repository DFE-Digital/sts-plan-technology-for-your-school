using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

public interface IProcessSubmissionResponsesCommand
{
    public Task<SubmissionResponsesDto?> GetSubmissionResponsesDtoForSection(int establishmentId, ISectionComponent section, bool completed = false, CancellationToken cancellationToken = default);
}
