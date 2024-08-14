using System.Text.RegularExpressions;

namespace Dfe.PlanTech.Web.Helpers;

public static partial class ViewHelpers
{
    private const string RemoveNonAlphanumericCharactersRegexPattern = @"[^a-zA-Z0-9\s]";

    public static string Slugify(this string text)
    => RemoveNonAlphaNumericCharactersPattern().Replace(text, "").Replace(" ", "-").ToLower();

    [GeneratedRegex(RemoveNonAlphanumericCharactersRegexPattern)]
    private static partial Regex RemoveNonAlphaNumericCharactersPattern();
}
