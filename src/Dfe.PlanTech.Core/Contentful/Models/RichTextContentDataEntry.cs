using System.Diagnostics.CodeAnalysis;
using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class RichTextContentDataEntry : Entry<ContentfulEntry>
{
    public string Id => SystemProperties.Id;
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string? Title { get; init; }
    public Asset? Asset { get; init; }
    public IReadOnlyList<RichTextContentDataEntry> Content { get; init; } = [];
    public string SummaryLine { get; init; } = null!;
    public string? Uri { get; init; } = null!;
    public RichTextContentField RichText { get; init; } = null!;
}
