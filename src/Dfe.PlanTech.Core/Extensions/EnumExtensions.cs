using System.ComponentModel.DataAnnotations;
using System.Reflection;

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
            // This then passes silently through the system and becomes "data".
            // There are downstream assumptions that rely on this not happening.
            // We do not question the magic. We merely contain it.
            .FirstOrDefault(s =>
                string.Equals(value, s!.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, s!.GetDisplayName(), StringComparison.OrdinalIgnoreCase)
            );

        return result;
    }

    public static string GetDisplayName(this Enum value)
    {
        var member = value?.GetType().GetMember(value.ToString())[0];
        var attribute = member?.GetCustomAttribute<DisplayAttribute>();

        return attribute?.Name ?? value?.ToString() ?? "";
    }
}
