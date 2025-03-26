﻿using Contentful.Core.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class CardComponentContextData : ContentComponent, IContentComponent, ICardComponentData
{
    public string InternalName { get; init; } = null!;
    public string? Title { get; init; } = null!;

    public string? Meta { get; init; } = null!;

    public Asset? Image { get; init; } = null!;

    public string? ImageAlt { get; init; } = null!;

    public string? Uri { get; init; } = null!;
}

public class RichTextContentData : Entry<ContentComponent>, IContentComponent, IHasSlug, IRichTextData
{
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string? Title { get; init; }
    public string? Description { get; }
    public string? Meta { get; }
    public Asset Asset { get; init; } = null!;
    public IReadOnlyList<RichTextContentData> Content { get; init; } = [];
    public string SummaryLine { get; init; } = null!;
    public string? Uri { get; init; } = null!;    
    public SystemDetails Sys { get; init; } = null!;
    public RichTextContent RichText { get; init; } = null!;
}
