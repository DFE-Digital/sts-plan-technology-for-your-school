using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces
{
    public interface IEstablishmentRecommendationHistoryRepository
    {
        Task<List<EstablishmentRecommendationHistoryEntity>> GetRecommendationHistoryByEstablishmentIdAsync(int establishmentId);
    }
}
