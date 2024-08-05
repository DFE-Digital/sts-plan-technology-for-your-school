using System.Text.RegularExpressions;

namespace Dfe.PlanTech;

public static partial class StringHelpers
{
    private const string RemoveNonAlphanumericExceptHyphensRegexPattern = @"[^a-zA-Z0-9\s-]";

    // Extension method to slugify a string by removing non-alphanumeric characters (except hyphens to preserve hyphenated words), replacing spaces with hyphens, and converting to lowercase.
    public static string Slugify(this string text)
    => RemoveNonAlphaNumericExceptHyphensPattern().Replace(text, "").Replace(" ", "-").ToLower();

    public static string TruncateIfOverLength(this string text, int maxLength) => text.Length < maxLength ? text : text[..maxLength];

    [GeneratedRegex(RemoveNonAlphanumericExceptHyphensRegexPattern)]
    private static partial Regex RemoveNonAlphaNumericExceptHyphensPattern();
}
