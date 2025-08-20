using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces
{
    public interface IGroupReadActivityRepository
    {
        Task<List<GroupReadActivityEntity>> GetGroupReadActivitiesAsync(int userId, int userEstablishmentId);
    }
}