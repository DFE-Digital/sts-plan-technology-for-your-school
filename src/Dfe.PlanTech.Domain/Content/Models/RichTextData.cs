using Contentful.Core.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Data for a RichText section
/// </summary>
/// <inheritdoc/>
public class RichTextData : IRichTextData
{
    public string? Uri { get; init; }
}

public class CustomData : IRichTextData
{
    public string? Uri { get; init; }
    public RichTextContentData Target { get; init; }
}

public class RichTextContentData : Entry<ContentBase>, IContentComponent, IHasSlug, IRichTextData
{
    public string InternalName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public Asset Asset { get; set; } = null!;
    public List<RichTextContentData> Content { get; set; } = [];
    public string SummaryLine { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Meta { get; set; } = null!;
    public string ImageAlt { get; set; } = null!;
    public string? Uri { get; init; } = null!;
    public Image Image { get; set; } = null!;
    public SystemDetails Sys { get; set; } = null!;
    public RichTextContent RichText { get; set; } = null!;
}
