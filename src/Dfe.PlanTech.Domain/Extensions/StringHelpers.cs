using System.Text.RegularExpressions;

namespace Dfe.PlanTech;

public static partial class StringHelpers
{
    private const string MatchNonAlphanumericExceptHyphensRegexPattern = @"[^a-zA-Z0-9-]";
    private const string MatchWhitespaceCharacters = @"\s";

    [GeneratedRegex(MatchNonAlphanumericExceptHyphensRegexPattern)]
    private static partial Regex MatchNonAlphaNumericExceptHyphensPattern();

    [GeneratedRegex(MatchWhitespaceCharacters)]
    private static partial Regex MatchWhitespaceCharactersPattern();

    // Extension method to slugify a string by removing non-alphanumeric characters (except hyphens to preserve hyphenated words), replacing spaces with hyphens, and converting to lowercase.
    public static string Slugify(this string text) => text.Trim().ReplaceWhitespaceCharacters("-").ReplaceNonSlugCharacters("").ToLower();

    private static string ReplaceWhitespaceCharacters(this string text, string replacement)
    => MatchWhitespaceCharactersPattern().Replace(text, replacement);

    private static string ReplaceNonSlugCharacters(this string text, string replacement)
    => MatchNonAlphaNumericExceptHyphensPattern().Replace(text, replacement);
}
