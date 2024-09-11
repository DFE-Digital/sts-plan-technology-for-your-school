using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

public interface IDeleteCurrentSubmissionCommand
{
    public Task DeleteCurrentSubmission(ISectionComponent section, CancellationToken cancellationToken = default);
}
