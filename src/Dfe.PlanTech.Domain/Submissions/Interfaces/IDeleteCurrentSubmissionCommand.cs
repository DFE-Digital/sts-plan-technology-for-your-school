using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

public interface IDeleteCurrentSubmissionCommand
{
    public Task DeleteCurrentSubmission(Section section, CancellationToken cancellationToken = default);
}
