using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;

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

        var recommendationHistoryEntities = await establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId);

        var latestHistoryForRecommendation = recommendationHistoryEntities
            .Where(rhe => rhe.RecommendationId == recommendation.Id)
            .OrderByDescending(r => r.DateCreated)
            .FirstOrDefault();

        return latestHistoryForRecommendation?.AsDto();
    }

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
