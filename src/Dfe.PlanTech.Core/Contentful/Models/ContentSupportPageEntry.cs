using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class ContentSupportPageEntry: TransformableEntry<ContentSupportPageEntry, CmsContentSupportPageDto>, IContentfulEntry
{
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public ComponentHeroEntry Heading { get; init; } = null!;
    public IEnumerable<Entry<ContentComponent>> Content { get; init; } = [];
    public bool IncludeInSiteMap { get; init; } = false;
    public bool HasBackToTop { get; init; } = false;
    public bool HasCitation { get; init; } = false;
    public bool ShowVerticalNavigation { get; init; } = false;
    public bool HasFeedbackBanner { get; init; } = false;
    public bool HasPrint { get; init; } = false;

    public ContentSupportPageEntry() : base(entry => new CmsContentSupportPageDto(entry)) { }
}
