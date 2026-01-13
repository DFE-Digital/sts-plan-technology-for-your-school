using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Core.Utilities;

public static class RecommendationSorter
{
    public static List<RecommendationChunkEntry> SortByStatus(
        this IEnumerable<RecommendationChunkEntry> chunks,
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto> history,
        RecommendationSortOrder sortType)
    {
        var indexed = chunks.Select((chunk, index) => new { chunk, index });

        return sortType switch
        {
            RecommendationSortOrder.Default =>
                indexed.Select(x => x.chunk).ToList(),

            RecommendationSortOrder.Status =>
                indexed.OrderBy(x => RecommendationStatusHelper.GetStatus(x.chunk, history))
                       .ThenBy(x => x.index)
                       .Select(x => x.chunk)
                       .ToList(),

            RecommendationSortOrder.LastUpdated =>
                indexed.OrderByDescending(x => RecommendationStatusHelper.GetLastUpdatedUtc(x.chunk, history))
                       .ThenBy(x => x.index)
                       .Select(x => x.chunk)
                       .ToList(),

            _ => indexed.Select(x => x.chunk).ToList()
        };
    }
}
