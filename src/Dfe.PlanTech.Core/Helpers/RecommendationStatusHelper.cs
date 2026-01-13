using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.Helpers
{
    public static class RecommendationStatusHelper
    {
        public static RecommendationStatus? GetRecommendationStatusEnumValue(this string recommendationStatus)
        {
            return Enum.GetValues<RecommendationStatus>()
                .Cast<RecommendationStatus?>()
                .FirstOrDefault(rs => string.Equals(rs!.ToString(), recommendationStatus, StringComparison.InvariantCultureIgnoreCase));
        }

        public static RecommendationStatus GetStatus(
            RecommendationChunkEntry chunk,
            Dictionary<string, SqlEstablishmentRecommendationHistoryDto> history
        )
        {
            var status = history.TryGetValue(chunk.Id, out var recommendationHistory)
               ? recommendationHistory.NewStatus
               : RecommendationStatus.NotStarted.ToString();

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
