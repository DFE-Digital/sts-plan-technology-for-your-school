using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

public interface IProcessSubmissionResponsesCommand
{
    public Task<SubmissionResponsesDto?> GetSubmissionResponsesDtoForSection(int establishmentId, ISectionComponent section, CancellationToken cancellationToken = default);
}
