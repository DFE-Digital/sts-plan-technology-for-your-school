using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsNavigationLinkDto : CmsEntryDto
    {
        public string Id { get; set; } = null!;
        public string InternalName { get; set; } = null!;
        public string DisplayText { get; set; } = null!;
        public string? Href { get; set; } = null;
        public bool OpenInNewTab { get; set; } = false;

        // Can't work out where this comes from based on the previous codebase
        //public ContentComponent? ContentToLinkTo { get; set; }

        public CmsNavigationLinkDto(NavigationLinkEntry navigationLinkEntry)
        {
            Id = navigationLinkEntry.Id;
            InternalName = navigationLinkEntry.InternalName;
            DisplayText = navigationLinkEntry.DisplayText;
            Href = navigationLinkEntry.Href;
            OpenInNewTab = navigationLinkEntry.OpenInNewTab;
        }
    }
}
