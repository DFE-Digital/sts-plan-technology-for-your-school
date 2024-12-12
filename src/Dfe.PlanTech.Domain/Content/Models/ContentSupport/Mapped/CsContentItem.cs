using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;

[ExcludeFromCodeCoverage]
public class CsContentItem : IContentComponent
{
    public string InternalName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Title { get; set; } = null;
    public string? Subtitle { get; set; } = null;
    public bool UseParentHero { get; set; }

    public SystemDetails Sys { get; set; } = new SystemDetails();
}
