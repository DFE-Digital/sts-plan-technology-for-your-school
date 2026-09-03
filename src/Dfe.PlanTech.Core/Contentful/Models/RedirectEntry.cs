using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class RedirectEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string RedirectFrom { get; init; } = null!;
    public string RedirectTo { get; init; } = null!;

    // Split by newline and strip first forward slash from all resulting slugs
    public IEnumerable<string> RedirectFromList =>
        Regex
            // Split by newline
            .Split(RedirectFrom, @"[\n\r]+", RegexOptions.Compiled, TimeSpan.FromSeconds(2))
            // Remove the leading forward slash, then remove whitespace
            .Select(stem => stem[1..].Trim());
}
