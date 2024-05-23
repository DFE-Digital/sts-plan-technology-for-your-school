using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

public interface IResetSubmissionCommand
{
    public Task ResetSubmission(Section section, CancellationToken cancellationToken = default);
}
