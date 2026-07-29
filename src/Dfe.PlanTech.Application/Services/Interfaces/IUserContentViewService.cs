namespace Dfe.PlanTech.Application.Services.Interfaces
{
    public interface IUserContentViewService
    {
        Task<int> GetNumberOfTimesContentViewedByUserAsync(string contentfulRef);
        Task RecordContentViewAsync(string contentfulRef);
    }
}
