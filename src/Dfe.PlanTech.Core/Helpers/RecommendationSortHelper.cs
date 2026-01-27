using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Core.Helpers
{
    public static class RecommendationSortHelper
    {
        public static RecommendationSortOrder GetRecommendationSortEnumValue(this string? sortOrder)
        {
            if (sortOrder is null)
            {
                return RecommendationSortOrder.Default;
            }

            return Enum.GetValues<RecommendationSortOrder>()
                    .Cast<RecommendationSortOrder?>()
                    .FirstOrDefault(s =>
                        string.Equals(
                            sortOrder,
                            s!.GetDisplayName(),
                            StringComparison.InvariantCultureIgnoreCase
                        )
                        || string.Equals(
                            sortOrder,
                            s!.ToString(),
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    )
                ?? RecommendationSortOrder.Default;
        }
    }
}
