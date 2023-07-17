namespace Dfe.PlanTech.Application.Submission.Interfaces
{
    public interface IGetSubmissionStatusesQuery
    {
        Task<IDictionary<string, string>> GetSectionSubmissionStatuses();
    }
}
