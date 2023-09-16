namespace Dfe.PlanTech.Application.Submissions.Interfaces
{
    public interface ICreateSubmissionCommand
    {
        Task<int> CreateSubmission(Domain.Submissions.Models.Submission submission);
    }
}
