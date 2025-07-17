namespace Dfe.PlanTech.Domain.Submissions.Interfaces
{
    public interface IMarkSubmissionAsReviewedCommand
    {
        Task MarkSubmissionAsReviewed(int submissionId, CancellationToken cancellationToken);
    }
}
