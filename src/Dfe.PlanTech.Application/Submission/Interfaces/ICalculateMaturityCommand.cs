namespace Dfe.PlanTech.Application.Submission.Interfaces
{
    public interface ICalculateMaturityCommand
    {
        Task<int> CalculateMaturityAsync(int submissionId, CancellationToken cancellationToken = default);
    }
}
