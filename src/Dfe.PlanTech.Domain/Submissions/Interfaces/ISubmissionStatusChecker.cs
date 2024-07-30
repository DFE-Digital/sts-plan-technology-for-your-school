namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

public interface ISubmissionStatusChecker
{
    public bool IsMatchingSubmissionStatus(ISubmissionStatusProcessor statusProcessor);
    public Task ProcessSubmission(ISubmissionStatusProcessor statusProcessor, CancellationToken cancellationToken);
}
