using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class RecommendationWorkflow(
    IEstablishmentRecommendationHistoryRepository establishmentRecommendationHistoryRepository,
    IRecommendationRepository recommendationRepository
) : IRecommendationWorkflow
{
    public async Task<SqlEstablishmentRecommendationHistoryDto?> GetCurrentRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId)
    {
        var recommendations = await recommendationRepository.GetRecommendationsByContentfulReferencesAsync(new[] { recommendationContentfulReference });
        var recommendation = recommendations.FirstOrDefault();

        if (recommendation == null)
        {
            return null;
        }

        var latestHistoryForRecommendation = await establishmentRecommendationHistoryRepository.GetLatestRecommendationHistoryAsync(establishmentId, recommendation.Id);

        return latestHistoryForRecommendation?.AsDto();
    }

    public async Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesByRecommendationIdAsync(IEnumerable<string> recommendationContentfulReferences,
        int establishmentId)
    {
        var recommendationHistoryEntities = await establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId);
        Dictionary<string,SqlEstablishmentRecommendationHistoryDto> x;

        return recommendationHistoryEntities
            .GroupBy(rhe => rhe.Recommendation.ContentfulRef)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(rhe => rhe.DateCreated).First().AsDto()
            );
    }

    public async Task UpdateRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId,
        int userId,
        string newStatus,
        string? noteText = null,
        int? matEstablishmentId = null)
    {
        // Get the recommendation by ContentfulRef to get its ID
        var recommendations = await recommendationRepository.GetRecommendationsByContentfulReferencesAsync(new[] { recommendationContentfulReference });
        var recommendation = recommendations.FirstOrDefault()
            ?? throw new InvalidOperationException($"Recommendation with ContentfulRef '{recommendationContentfulReference}' not found");

        // Get current status, to use it as the new previous status
        var currentStatus = await GetCurrentRecommendationStatusAsync(recommendationContentfulReference, establishmentId);
        var previousStatus = currentStatus?.NewStatus;

        await establishmentRecommendationHistoryRepository.CreateRecommendationHistoryAsync(
            establishmentId,
            recommendation.Id,
            userId,
            matEstablishmentId,
            previousStatus,
            newStatus,
            noteText ?? string.Empty
        );
    }
}
