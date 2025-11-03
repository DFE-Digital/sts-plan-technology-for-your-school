using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Core.Helpers
{
    public static class RecommendationSortHelper
    {
        public static RecommendationSort GetRecommendationSortEnumValue(this string? sortOrder)
        {
            if (sortOrder is null)
            {
                return RecommendationSort.Default;
            }

            return Enum.GetValues<RecommendationSort>()
                .Cast<RecommendationSort?>()
                .FirstOrDefault(s => string.Equals(sortOrder, s!.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase))
                ?? RecommendationSort.Default;
        }
    }
}
