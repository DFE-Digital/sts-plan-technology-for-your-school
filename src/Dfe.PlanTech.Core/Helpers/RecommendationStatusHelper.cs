using System.Reflection;
using Dfe.PlanTech.Core.Attributes;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Core.Helpers;

public static class RecommendationStatusHelper
{
    public static RecommendationStatus? ToRecommendationStatus(this string? recommendationStatus)
    {
        return recommendationStatus.GetEnumValue<RecommendationStatus>();
    }

    public static string GetCssClassOrDefault(this RecommendationStatus? value, string defaultValue)
    {
        if (value is null)
        {
            return defaultValue;
        }

        var member = value.GetType().GetMember(value.Value.ToString()).First();
        var attribute = member.GetCustomAttribute<CssClassAttribute>();

        return attribute?.ClassName ?? defaultValue;
    }

    public static string GetCssClass(this RecommendationStatus value)
    {
        return GetCssClassOrDefault(value, RecommendationConstants.DefaultTagClass);
    }
}
