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
    }
}
