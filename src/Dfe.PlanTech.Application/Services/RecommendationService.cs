using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Services;

public class RecommendationService(
    // IRecommendationWorkflow recommendationWorkflow
) : IRecommendationService
{
    public Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesByRecommendationIdAsync(
        IEnumerable<string> recommendationContentfulReferences,
        int establishmentId
    )
    {
        // Temporary stub
        var dict = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>();
        return Task.FromResult(dict);
        // return recommendationWorkflow.GetMostRecentRecommendationStatusesAsync(
        //     recommendationContentfulReferences,
        //     establishmentId
        // );
    }
}
