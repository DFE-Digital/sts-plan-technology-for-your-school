using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Dfe.PlanTech.Core.Attributes;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.Extensions;

public static class EnumExtensions
{
    public static TEnum? GetEnumValue<TEnum>(this string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var values = Enum.GetValues<TEnum>();
        var result = values
            // ⚠️ DO NOT REMOVE
            .Cast<TEnum?>()
            // Without this Cast(), LINQ will silently summon default(TEnum) (i.e. the zeroth value)
            // for non-matching inputs, which is wrong on many levels but is technically legal C#.
            // This new data then passes silently through the system and becomes "data".
            // There are downstream assumptions that rely on this not happening.
            // We do not question the magic. We merely contain it.
            .FirstOrDefault(s =>
                string.Equals(value, s!.ToString(), StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(value, s!.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase)
            );

        return result;
    }

    public static string GetDisplayName(this Enum value)
    {
        var member = value?.GetType().GetMember(value.ToString())[0];
        var attribute = member?.GetCustomAttribute<DisplayAttribute>();

        return attribute?.Name ?? value?.ToString() ?? "";
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
