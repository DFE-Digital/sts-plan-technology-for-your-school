namespace Dfe.PlanTech.Domain.Submissions.Interfaces;

    public interface ICalculateMaturityCommand
    {
        Task<int> CalculateMaturityAsync(int submissionId, CancellationToken cancellationToken = default);
    }
