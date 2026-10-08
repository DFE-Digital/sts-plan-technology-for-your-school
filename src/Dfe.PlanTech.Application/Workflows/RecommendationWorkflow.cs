using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class RecommendationWorkflow(
    IEstablishmentRecommendationHistoryRepository establishmentRecommendationHistoryRepository,
    IRecommendationRepository recommendationRepository,
    IStoredProcedureRepository storedProcedureRepository
) : IRecommendationWorkflow
{
    public async Task<SqlEstablishmentRecommendationHistoryDto?> GetLatestRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId
    )
    {
        var recommendations =
            await recommendationRepository.GetRecommendationsByContentfulReferencesAsync([
                recommendationContentfulReference,
            ]);
        var recommendation = recommendations.FirstOrDefault();
        if (recommendation == null)
        {
            return null;
        }

        var latestHistoryForRecommendation =
            await establishmentRecommendationHistoryRepository.GetLatestRecommendationHistoryAsync(
                establishmentId,
                recommendation.Id
            );

        return latestHistoryForRecommendation?.AsDto();
    }

    public async Task<
        IEnumerable<SqlEstablishmentRecommendationHistoryDto>
    > GetRecommendationHistoryAsync(string recommendationContentfulReference, int establishmentId)
    {
        var recommendations =
            await recommendationRepository.GetRecommendationsByContentfulReferencesAsync([
                recommendationContentfulReference,
            ]);
        var recommendation = recommendations.FirstOrDefault();
        if (recommendation == null)
        {
            return [];
        }

        var latestHistoryForRecommendation =
            await establishmentRecommendationHistoryRepository
                .GetRecommendationHistoryByEstablishmentIdAndRecommendationIdAsync(
                    establishmentId,
                    recommendation.Id
                );

        return latestHistoryForRecommendation.Select(lhfr => lhfr.AsDto());
    }

    public async Task<
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
    > GetLatestRecommendationStatusesByEstablishmentIdAsync(int establishmentId)
    {
        var recommendationHistoryEntities =
            await establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(
                establishmentId
            );

        return recommendationHistoryEntities
            .GroupBy(rhe => rhe.Recommendation.ContentfulRef)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(g => g.DateCreated).First().AsDto()
            );
    }

    public async Task UpdateRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId,
        int userId,
        RecommendationStatus newStatus,
        string? noteText = null,
        int? matEstablishmentId = null,
        int? responseId = null
    )
    {
        // Get the recommendation by ContentfulRef to get its ID
        var recommendations =
            await recommendationRepository.GetRecommendationsByContentfulReferencesAsync(
                new[] { recommendationContentfulReference }
            );
        var recommendation =
            recommendations.FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"Recommendation with ContentfulRef '{recommendationContentfulReference}' not found"
            );

        // Get current status, to use it as the new previous status
        var currentStatus = await GetLatestRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId
        );
        var previousStatus = currentStatus?.NewStatus;

        await establishmentRecommendationHistoryRepository.CreateRecommendationHistoryAsync(
            establishmentId,
            recommendation.Id,
            userId,
            matEstablishmentId,
            responseId,
            previousStatus,
            newStatus,
            noteText ?? string.Empty
        );
    }

    public async Task UpdateEstablishmentsRecommendationStatusAsync(
        string recommendationContentfulReference,
        IEnumerable<int> establishmentIds,
        int userId,
        RecommendationStatus newStatus,
        string? noteText = null,
        int? matEstablishmentId = null,
        int? responseId = null
    )
    {
        var distinctEstablishmentIds = establishmentIds
            .Distinct()
            .ToArray();

        if (distinctEstablishmentIds.Length == 0)
        {
            return;
        }

        // Get the recommendation once
        var recommendations =
            await recommendationRepository.GetRecommendationsByContentfulReferencesAsync(
                [recommendationContentfulReference]
            );

        var recommendation =
            recommendations.FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"Recommendation with ContentfulRef '{recommendationContentfulReference}' not found"
            );

        // Get the latest recommendation history for all establishments in one query
        var latestHistories =
            await establishmentRecommendationHistoryRepository
                .GetLatestRecommendationHistoriesAsync(
                    distinctEstablishmentIds,
                    recommendation.Id
                );

        var latestHistoryByEstablishmentId = latestHistories
            .ToDictionary(
                history => history.EstablishmentId,
                history => history
            );

        // Build one new history record per establishment
        var histories = distinctEstablishmentIds
            .Select(establishmentId =>
            {
                latestHistoryByEstablishmentId.TryGetValue(
                    establishmentId,
                    out var latestHistory
                );

                return new EstablishmentRecommendationHistoryEntity()
                {
                    EstablishmentId = establishmentId,
                    RecommendationId = recommendation.Id,
                    UserId = userId,
                    MatEstablishmentId = matEstablishmentId,
                    ResponseId = responseId,
                    PreviousStatus = latestHistory?.NewStatus,
                    NewStatus = newStatus,
                    NoteText = noteText ?? string.Empty,
                    DateCreated = DateTime.UtcNow
                };
            })
            .ToList();

        // AddRange + single SaveChanges
        await establishmentRecommendationHistoryRepository
            .CreateRecommendationHistoriesAsync(histories);
    }

    public async Task<SqlFirstActivityForEstablishmentRecommendationDto?>
        GetFirstActivityForEstablishmentRecommendationAsync(
            int establishmentId,
            string recommendationContentfulReference
        )
    {
        var firstActivity =
            await storedProcedureRepository.GetFirstActivityForEstablishmentRecommendationAsync(
                establishmentId,
                recommendationContentfulReference
            );

        return firstActivity?.AsDto();
    }

    public async Task<IEnumerable<SqlRecommendationDto>> GetRecommendationsByContentfulReferencesAsync(
        IEnumerable<string> recommendationContentfulReferences)
    {
        var recommendations =
            await recommendationRepository.GetRecommendationsByContentfulReferencesAsync(
                recommendationContentfulReferences);
        return recommendations.Select(r => r.AsDto());
    }
}
