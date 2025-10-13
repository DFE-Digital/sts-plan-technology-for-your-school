using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Core.Utilities;

public static class RecommendationSorter
{
    private static RecommendationStatus GetStatus(
        RecommendationChunkEntry chunk,
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto> history)
    {
        var status = history.TryGetValue(chunk.Id, out var recommendationHistory)
           ? recommendationHistory.NewStatus
           : RecommendationStatus.NotStarted.GetDisplayName();

        return status.GetRecommendationStatusEnumValue() ?? RecommendationStatus.NotStarted;
    }

    private static DateTime GetLastUpdatedUtc(
        RecommendationChunkEntry entry,
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto> history)
    {
        return history.TryGetValue(entry.Id, out var recommendationHistory)
            ? recommendationHistory.DateCreated
            : DateTime.MinValue;
    }

    public static List<RecommendationChunkEntry> SortByStatus(
        this IEnumerable<RecommendationChunkEntry> chunks,
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto> history,
        RecommendationSort sortType)
    {
        var indexed = chunks.Select((chunk, index) => new { chunk, index });

        return sortType switch
        {
            RecommendationSort.Status =>
                indexed.OrderBy(x => GetStatus(x.chunk, history))
                       .ThenBy(x => x.index)
                       .Select(x => x.chunk)
                       .ToList(),

            RecommendationSort.LastUpdated =>
                indexed.OrderByDescending(x => GetLastUpdatedUtc(x.chunk, history))
                       .ThenBy(x => x.index)
                       .Select(x => x.chunk)
                       .ToList(),

            _ => indexed.Select(x => x.chunk).ToList() // For 'Default' and null cases
        };
    }
}
