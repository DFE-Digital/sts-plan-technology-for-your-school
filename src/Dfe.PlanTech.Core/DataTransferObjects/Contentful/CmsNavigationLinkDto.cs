using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsNavigationLinkDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public CmsEntryDto? ContentToLinkTo { get; set; } = null!;
        public string InternalName { get; set; } = null!;
        public string DisplayText { get; set; } = null!;
        public string? Href { get; set; } = null;
        public bool OpenInNewTab { get; set; } = false;

        public bool IsValid => !string.IsNullOrEmpty(DisplayText) && !(string.IsNullOrEmpty(Href) && ContentToLinkTo == null);

        // Can't work out where this comes from based on the previous codebase
        //public ContentComponent? ContentToLinkTo { get; set; }

        public CmsNavigationLinkDto(NavigationLinkEntry navigationLinkEntry)
        {
            Id = navigationLinkEntry.Id;
            ContentToLinkTo = navigationLinkEntry.ContentToLinkTo is null
                ? null
                : BuildContentDto(navigationLinkEntry.ContentToLinkTo);
            InternalName = navigationLinkEntry.InternalName;
            DisplayText = navigationLinkEntry.DisplayText;
            Href = navigationLinkEntry.Href;
            OpenInNewTab = navigationLinkEntry.OpenInNewTab;
        }
    }
}
