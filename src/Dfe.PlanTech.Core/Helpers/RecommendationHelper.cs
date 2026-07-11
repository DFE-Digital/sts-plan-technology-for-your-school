using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.Helpers;

public static class RecommendationHelper
{
    public static DateTime GetLastUpdatedUtc(
        this RecommendationChunkEntry entry,
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto> history
    )
    {
        return history.TryGetValue(entry.Id, out var recommendationHistory)
            ? recommendationHistory.DateCreated
            : DateTime.MinValue;
    }

    public static RecommendationStatus GetStatus(
        this RecommendationChunkEntry chunk,
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto> history
    )
    {
        var status = history.TryGetValue(chunk.Id, out var recommendationHistory)
            ? recommendationHistory.NewStatus
            : RecommendationStatus.NotStarted;

        return status ?? RecommendationStatus.NotStarted;
    }

    public static List<RecommendationChunkEntry> SortByStatus(
        this IEnumerable<RecommendationChunkEntry> chunks,
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto> history,
        RecommendationSortOrder sortType
    )
    {
        var indexed = chunks.Select((chunk, index) => new { chunk, index });

        return sortType switch
        {
            RecommendationSortOrder.Default => indexed.Select(x => x.chunk).ToList(),

            RecommendationSortOrder.Status => indexed
                .OrderBy(x => x.chunk.GetStatus(history))
                .ThenBy(x => x.index)
                .Select(x => x.chunk)
                .ToList(),

            RecommendationSortOrder.LastUpdated => indexed
                .OrderByDescending(x => x.chunk.GetLastUpdatedUtc(history))
                .ThenBy(x => x.index)
                .Select(x => x.chunk)
                .ToList(),

            _ => indexed.Select(x => x.chunk).ToList(),
        };
    }
}
