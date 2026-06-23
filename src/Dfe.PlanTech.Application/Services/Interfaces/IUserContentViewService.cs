namespace Dfe.PlanTech.Application.Services
{
    public interface IUserContentViewService
    {
        Task<int> GetNumberOfTimesContentViewedByUserAsync(string contentfulRef);
        Task RecordContentViewAsync(string contentfulRef);
    }
}
