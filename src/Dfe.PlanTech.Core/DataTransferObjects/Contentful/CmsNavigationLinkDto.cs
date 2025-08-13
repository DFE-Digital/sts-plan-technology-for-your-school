using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsNavigationLinkDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public CmsEntryDto? ContentToLinkTo { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public string DisplayText { get; set; } = null!;
    public string? Href { get; set; } = null;
    public bool OpenInNewTab { get; set; } = false;

    // To be valid, a link entry must contain display text, and must have either a href or some content to link to
    public bool IsValid => !string.IsNullOrEmpty(DisplayText) && (!string.IsNullOrEmpty(Href) || ContentToLinkTo is not null);

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
