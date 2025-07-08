using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class PageEntry : Entry<ContentComponent>
{
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public bool DisplayHomeButton { get; init; }
    public bool DisplayBackButton { get; init; }
    public bool DisplayOrganisationName { get; init; }
    public bool DisplayTopicTitle { get; init; }
    public List<ContentComponent> BeforeTitleContent { get; init; } = [];
    public ComponentTitleEntry? Title { get; init; }
    public List<ContentComponent> Content { get; init; } = [];
    public bool RequiresAuthorisation { get; init; } = true;
}
