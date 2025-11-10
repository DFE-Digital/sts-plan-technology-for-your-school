using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Dfe.PlanTech.Core.Attributes;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).First();
        var attribute = member.GetCustomAttribute<DisplayAttribute>();

        return attribute?.Name ?? value.ToString();
    }

    public static string GetDescription(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).First();
        var attribute = member.GetCustomAttribute<DisplayAttribute>();

        return attribute?.Description ?? value.ToString().ToLowerInvariant();
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
