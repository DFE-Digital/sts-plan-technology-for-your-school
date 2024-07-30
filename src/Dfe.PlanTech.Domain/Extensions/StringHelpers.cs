using System.Text.RegularExpressions;

namespace Dfe.PlanTech;

public static partial class StringHelpers
{
    private const string RemoveNonAlphanumericCharactersRegexPattern = @"[^a-zA-Z0-9\s]";

    // Extension method to slugify a string by removing non-alphanumeric characters, replacing spaces with hyphens, and converting to lowercase.
    public static string Slugify(this string text)
    => RemoveNonAlphaNumericCharactersPattern().Replace(text, "").Replace(" ", "-").ToLower();

    public static string TruncateIfOverLength(this string text, int maxLength) => text.Length < maxLength ? text : text[..maxLength];

    [GeneratedRegex(RemoveNonAlphanumericCharactersRegexPattern)]
    private static partial Regex RemoveNonAlphaNumericCharactersPattern();
}
