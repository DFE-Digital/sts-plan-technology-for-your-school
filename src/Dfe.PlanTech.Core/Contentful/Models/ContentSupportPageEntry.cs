using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class ContentSupportPageEntry : ContentfulEntry
{
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public ComponentHeroEntry Heading { get; init; } = null!;
    public IEnumerable<ContentfulEntry> Content { get; init; } = [];
    public bool IncludeInSiteMap { get; init; } = false;
    public bool HasBackToTop { get; init; } = false;
    public bool HasCitation { get; init; } = false;
    public bool ShowVerticalNavigation { get; init; } = false;
    public bool HasFeedbackBanner { get; init; } = false;
    public bool HasPrint { get; init; } = false;
}
