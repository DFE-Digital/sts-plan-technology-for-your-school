using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class RichTextField : ContentComponent
{
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string? Title { get; init; }
    public Asset Asset { get; init; } = null!;
    public IReadOnlyList<RichTextField> Content { get; init; } = [];
    public string SummaryLine { get; init; } = null!;
    public string? Uri { get; init; } = null!;
    public SystemDetails Sys { get; init; } = null!;
    public RichTextContent RichText { get; init; } = null!;
}
