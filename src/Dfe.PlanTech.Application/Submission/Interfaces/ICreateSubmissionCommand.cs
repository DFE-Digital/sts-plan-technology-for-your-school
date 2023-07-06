namespace Dfe.PlanTech.Application.Submission.Interfaces
{
    public interface ICreateSubmissionCommand
    {
        Task<int> CreateSubmission(Domain.Submissions.Models.Submission submission);
    }
}
