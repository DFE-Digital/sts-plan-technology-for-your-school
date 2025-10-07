using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class RecommendationWorkflow(
    IEstablishmentRecommendationHistoryRepository establishmentRecommendationHistoryRepository,
    IRecommendationRepository recommendationRepository
) : IRecommendationWorkflow
{
    public async Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesByRecommendationIdAsync(IEnumerable<string> recommendationContentfulReferences,
        int establishmentId)
    {
        var recommendations = await recommendationRepository.GetRecommendationsByContentfulReferencesAsync(recommendationContentfulReferences);
        var recommendationIdToContentfulReferenceDictionary = recommendations
            .ToDictionary(
                r => r.Id,
                r => r.ContentfulRef
            );

        var recommendationHistoryEntities = await establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId);

        return recommendationHistoryEntities
            .GroupBy(rhe => rhe.RecommendationId)
            .ToDictionary(
                group => recommendationIdToContentfulReferenceDictionary[group.Key],
                group => group.OrderByDescending(r => r.DateCreated).First().AsDto()
            );
    }
}
