using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IRecommendationService
{
    Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesByRecommendationIdAsync(
        IEnumerable<string> recommendationContentfulReferences,
        int establishmentId
    );
}
