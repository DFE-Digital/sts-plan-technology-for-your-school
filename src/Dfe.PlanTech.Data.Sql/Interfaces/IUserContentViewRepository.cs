namespace Dfe.PlanTech.Data.Sql.Interfaces
{
    public interface IUserContentViewRepository
    {
        Task CreateAsync(int userId, string contentfulRef);
        Task<int> GetNumberOfTimesContentViewedByUserAsync(int userId, string contentfulRef);
    }
}
