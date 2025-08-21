using System.Net;
using System.Text.RegularExpressions;

namespace Dfe.PlanTech.Core.Extensions;

public static partial class StringExtensions
{
    public const string NonBreakingHyphen = "&#8209;";

    private const string MatchNonAlphanumericExceptHyphensRegexPattern = @"[^a-zA-Z0-9-]+";
    private const string MatchWhitespaceCharacters = @"\s+";

    [GeneratedRegex(MatchNonAlphanumericExceptHyphensRegexPattern)]
    private static partial Regex MatchNonAlphaNumericExceptHyphensPattern();

    [GeneratedRegex(MatchWhitespaceCharacters)]
    private static partial Regex MatchWhitespaceCharactersPattern();

    public static string FirstCharToUpper(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }
        Span<char> destination = stackalloc char[1];
        input.AsSpan(0, 1).ToUpperInvariant(destination);
        return $"{destination}{input.AsSpan(1)}";
    }

    public static string? UseNonBreakingHyphenAndHtmlDecode(this string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }
        return WebUtility.HtmlDecode(text.Replace("-", NonBreakingHyphen));
    }

    public static string FirstCharToLower(this string input) => char.ToLower(input[0]) + input[1..];

    // Extension method to slugify a string by removing non-alphanumeric characters (except hyphens to preserve hyphenated words), replacing spaces with hyphens, and converting to lowercase.
    public static string Slugify(this string text) => text.Trim().ReplaceWhitespaceCharacters("-").ReplaceNonSlugCharacters("").ToLower();

    private static string ReplaceWhitespaceCharacters(this string text, string replacement)
    => MatchWhitespaceCharactersPattern().Replace(text, replacement);

    private static string ReplaceNonSlugCharacters(this string text, string replacement)
    => MatchNonAlphaNumericExceptHyphensPattern().Replace(text, replacement);

}
