using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class PageEntry: TransformableEntry<PageEntry, CmsPageDto>
{
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public bool DisplayHomeButton { get; init; }
    public bool DisplayBackButton { get; init; }
    public bool DisplayOrganisationName { get; init; }
    public bool DisplayTopicTitle { get; init; }
    public bool RequiresAuthorisation { get; init; } = true;
    public string? SectionTitle { get; init; }
    public List<ContentfulEntry> BeforeTitleContent { get; init; } = [];
    public ComponentTitleEntry? Title { get; init; }
    public List<ContentfulEntry> Content { get; init; } = [];

    protected override Func<PageEntry, CmsPageDto> Constructor => entry => new(entry);
}
