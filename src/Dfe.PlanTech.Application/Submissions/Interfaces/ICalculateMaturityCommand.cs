namespace Dfe.PlanTech.Application.Submissions.Interfaces
{
    public interface ICalculateMaturityCommand
    {
        Task<int> CalculateMaturityAsync(int submissionId, CancellationToken cancellationToken = default);
    }
}
