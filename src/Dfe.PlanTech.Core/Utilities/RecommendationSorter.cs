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
        RecommendationSort sortType)
    {
        var indexed = chunks.Select((chunk, index) => new { chunk, index });

        return sortType switch
        {
            RecommendationSort.Default =>
                indexed.Select(x => x.chunk).ToList(),

            RecommendationSort.Status =>
                indexed.OrderBy(x => RecommendationHelper.GetStatus(x.chunk, history))
                       .ThenBy(x => x.index)
                       .Select(x => x.chunk)
                       .ToList(),

            RecommendationSort.LastUpdated =>
                indexed.OrderByDescending(x => RecommendationHelper.GetLastUpdatedUtc(x.chunk, history))
                       .ThenBy(x => x.index)
                       .Select(x => x.chunk)
                       .ToList(),

            _ => indexed.Select(x => x.chunk).ToList()
        };
    }
}
