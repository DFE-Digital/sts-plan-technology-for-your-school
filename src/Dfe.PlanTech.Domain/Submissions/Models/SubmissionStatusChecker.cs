using Dfe.PlanTech.Domain.Submissions.Interfaces;

namespace Dfe.PlanTech.Domain.Submissions.Models;

public class SubmissionStatusChecker : ISubmissionStatusChecker
{
    public Func<ISubmissionStatusProcessor, bool> IsMatchingSubmissionStatusFunc { get; init; } = null!;
    public Func<ISubmissionStatusProcessor, CancellationToken, Task> ProcessSubmissionFunc { get; init; } = null!;

    public bool IsMatchingSubmissionStatus(ISubmissionStatusProcessor statusProcessor) => IsMatchingSubmissionStatusFunc(statusProcessor);

    public Task ProcessSubmission(ISubmissionStatusProcessor statusProcessor, CancellationToken cancellationToken)
    => ProcessSubmissionFunc(statusProcessor, cancellationToken);
}
