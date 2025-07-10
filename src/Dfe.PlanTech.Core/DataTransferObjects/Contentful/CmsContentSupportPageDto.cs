using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsContentSupportPageDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; init; } = null!;
        public string Slug { get; init; } = null!;
        public CmsComponentHeroDto Heading { get; init; } = null!;
        public List<CmsEntryDto> Content { get; init; } = [];
        public bool IncludeInSiteMap { get; init; } = false;
        public bool HasBackToTop { get; init; } = false;
        public bool HasCitation { get; init; } = false;
        public bool ShowVerticalNavigation { get; init; } = false;
        public bool HasFeedbackBanner { get; init; } = false;
        public bool HasPrint { get; init; } = false;

        public CmsContentSupportPageDto(ContentSupportPageEntry supportPageEntry)
        {
            Id = supportPageEntry.Id;
            InternalName = supportPageEntry.InternalName;
            Slug = supportPageEntry.Slug;
            Heading = supportPageEntry.Heading.AsDto();
            Content = supportPageEntry.Content.AsDto();
            IncludeInSiteMap = supportPageEntry.IncludeInSiteMap;
            HasBackToTop = supportPageEntry.HasBackToTop;
            HasCitation = supportPageEntry.HasCitation;
            ShowVerticalNavigation = supportPageEntry.ShowVerticalNavigation;
            HasFeedbackBanner = supportPageEntry.HasFeedbackBanner;
            HasPrint = supportPageEntry.HasPrint;
        }
    }
}
