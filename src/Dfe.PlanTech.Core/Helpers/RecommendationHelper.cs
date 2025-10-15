using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Core.Helpers
{
    public static class RecommendationHelper
    {
        public static RecommendationStatus? GetRecommendationStatusEnumValue(this string recommendationStatus)
        {
            return Enum.GetValues(typeof(RecommendationStatus))
                .Cast<RecommendationStatus?>()
                .FirstOrDefault(rs => string.Equals(rs!.GetDisplayName(), recommendationStatus, StringComparison.InvariantCultureIgnoreCase));
        }

        public static RecommendationStatus GetStatus(
            RecommendationChunkEntry chunk,
            Dictionary<string, SqlEstablishmentRecommendationHistoryDto> history
        )
        {
            var status = history.TryGetValue(chunk.Id, out var recommendationHistory)
               ? recommendationHistory.NewStatus
               : RecommendationStatus.NotStarted.GetDisplayName();

            return status.GetRecommendationStatusEnumValue() ?? RecommendationStatus.NotStarted;
        }

        public static DateTime GetLastUpdatedUtc(
            RecommendationChunkEntry entry,
            Dictionary<string, SqlEstablishmentRecommendationHistoryDto> history
        )
        {
            return history.TryGetValue(entry.Id, out var recommendationHistory)
                ? recommendationHistory.DateCreated
                : DateTime.MinValue;
        }
    }
}
