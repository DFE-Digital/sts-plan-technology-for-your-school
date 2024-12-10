using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport;

[ExcludeFromCodeCoverage]
public class ContentBase : Contentful.Core.Models.Entry<ContentBase>, IContentComponent, IHasSlug
{
    public string InternalName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public SystemDetails Sys { get; set; } = null!;
    public Data Data { get; set; } = null!;
}
