namespace Dfe.PlanTech.Core.Contentful.Models;

public class PageEntry : ContentfulEntry
{
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public bool DisplayHomeButton { get; init; }
    public bool DisplayBackButton { get; init; }
    public bool DisplayOrganisationName { get; init; }
    public bool DisplayTopicTitle { get; init; }
    public bool? IsLandingPage { get; set; }
    public bool RequiresAuthorisation { get; init; } = true;
    public string? SectionTitle { get; init; }
    public List<ContentfulEntry> BeforeTitleContent { get; init; } = [];
    public ComponentTitleEntry? Title { get; init; }
    public List<ContentfulEntry>? Content { get; set; }
}
