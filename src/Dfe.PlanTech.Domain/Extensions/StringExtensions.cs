using System.Net;

namespace Dfe.PlanTech.Domain.Extensions;

public static class StringExtensions
{
    public const string NonBreakingHyphen = "&#8209;";
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
}
