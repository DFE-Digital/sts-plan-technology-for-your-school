using Contentful.Core.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Core.Content.Models;

public class RichTextContentData : Entry<ContentComponent>, IContentComponent, IHasSlug, IRichTextData
{
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string? Title { get; init; }
    public Asset Asset { get; init; } = null!;
    public IReadOnlyList<RichTextContentData> Content { get; init; } = [];
    public string SummaryLine { get; init; } = null!;
    public string? Uri { get; init; } = null!;
    public SystemDetails Sys { get; init; } = null!;
    public RichTextContent RichText { get; init; } = null!;
}
