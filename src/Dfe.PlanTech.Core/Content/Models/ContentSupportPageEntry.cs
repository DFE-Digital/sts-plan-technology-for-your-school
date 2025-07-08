using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class ContentSupportPageEntry : Entry<ContentComponent>
{
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public ComponentHeroEntry Heading { get; init; } = null!;
    public IEnumerable<ContentComponent> Content { get; init; } = [];
    public bool IncludeInSiteMap { get; init; } = false;
    public bool HasBackToTop { get; init; } = false;
    public bool HasCitation { get; init; } = false;
    public bool ShowVerticalNavigation { get; init; } = false;
    public bool HasFeedbackBanner { get; init; } = false;
    public bool HasPrint { get; init; } = false;

}
