using System.Text.RegularExpressions;

namespace Dfe.PlanTech.Web.Helpers;

public static partial class ViewHelpers
{
    private const string RemoveNonAlphanumericCharactersRegexPattern = @"[^a-zA-Z0-9\s]";

    public static string Slugify(this string text)
    {
        var slugifiedString = RemoveNonAlphaNumericCharactersPattern()
            .Replace(text, "")
            .Trim()
            .Replace(" ", "-")
            .ToLower();

        while (slugifiedString.Contains("--"))
        {
            slugifiedString = slugifiedString.Replace("--", "-");
        }

        return slugifiedString;
    }

    [GeneratedRegex(RemoveNonAlphanumericCharactersRegexPattern)]
    private static partial Regex RemoveNonAlphaNumericCharactersPattern();
}
