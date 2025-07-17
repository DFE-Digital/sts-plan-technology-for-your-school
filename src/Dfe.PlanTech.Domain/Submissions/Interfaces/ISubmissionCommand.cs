using Dfe.PlanTech.Domain.Submissions.Models;

public interface ISubmissionCommand
{
    Task<Submission> CloneSubmission(Submission? existingSubmission, CancellationToken cancellationToken);

    Task DeleteSubmission(int submissionId, CancellationToken cancellationToken);
}
