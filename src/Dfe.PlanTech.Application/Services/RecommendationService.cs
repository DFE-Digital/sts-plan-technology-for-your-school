using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Application.Services;

public class RecommendationService(IRecommendationWorkflow recommendationWorkflow)
    : IRecommendationService
{
    private readonly IRecommendationWorkflow _recommendationWorkflow = recommendationWorkflow
        ?? throw new ArgumentNullException(nameof(recommendationWorkflow));

    public Task<SqlEstablishmentRecommendationHistoryDto?> GetLatestRecommendationHistoryAsync(
        int establishmentId,
        string recommendationContentfulReference
    )
    {
        return _recommendationWorkflow.GetLatestRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId
        );
    }

    public Task<
        IEnumerable<SqlEstablishmentRecommendationHistoryDto>
    > GetRecommendationHistoryAsync(string recommendationContentfulReference, int establishmentId)
    {
        return _recommendationWorkflow.GetRecommendationHistoryAsync(
            recommendationContentfulReference,
            establishmentId
        );
    }

    public Task<
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
    > GetLatestRecommendationStatusesAsync(int establishmentId)
    {
        return _recommendationWorkflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(
            establishmentId
        );
    }

    public Task UpdateRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId,
        int userId,
        RecommendationStatus newStatus,
        string? noteText = null,
        int? matEstablishmentId = null
    )
    {
        return _recommendationWorkflow.UpdateRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentId,
            userId,
            newStatus,
            noteText,
            matEstablishmentId
        );
    }

    public Task<SqlFirstActivityForEstablishmentRecommendationDto?> GetFirstActivityForEstablishmentRecommendationAsync(
        int establishmentId,
        string recommendationContentfulReference
    )
    {
        return _recommendationWorkflow.GetFirstActivityForEstablishmentRecommendationAsync(
            establishmentId,
            recommendationContentfulReference
        );
    }

    public async Task<Dictionary<int, int>> GetInProgressOrCompletedRecommendationCounts(
        IEnumerable<int> establishmentIds
    )
    {
        var recommendationCounts = new Dictionary<int, int>();

        foreach (var establishmentId in establishmentIds)
        {
            recommendationCounts.Add(establishmentId, 0);

            var recommendations =
                await _recommendationWorkflow.GetLatestRecommendationStatusesByEstablishmentIdAsync(
                    establishmentId
                );

            recommendationCounts[establishmentId] = recommendations.Values.Count(r =>
                r.NewStatus == RecommendationStatus.Complete
                || r.NewStatus == RecommendationStatus.InProgress
            );
        }

        return recommendationCounts;
    }

    public async Task<
        IEnumerable<SqlRecommendationDto>
    > GetRecommendationsByContentfulReferencesAsync(
        IEnumerable<string> recommendationContentfulReferences
    )
    {
        return await _recommendationWorkflow.GetRecommendationsByContentfulReferencesAsync(
            recommendationContentfulReferences
        );
    }

    public Task UpdateEstablishmentsRecommendationStatusAsync(
        string recommendationContentfulReference,
        IEnumerable<int> establishmentIds,
        int userId,
        RecommendationStatus newStatus,
        string? noteText = null,
        int? matEstablishmentId = null,
        int? responseId = null
    )
    {
        return _recommendationWorkflow.UpdateEstablishmentsRecommendationStatusAsync(
            recommendationContentfulReference,
            establishmentIds,
            userId,
            newStatus,
            noteText,
            matEstablishmentId,
            responseId
        );
    }
}
