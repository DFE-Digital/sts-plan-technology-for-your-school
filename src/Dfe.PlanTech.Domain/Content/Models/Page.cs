using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class Page : ContentComponent, IPageContent
{
    public string? InternalName { get; init; }

    public string? Slug { get; init; }

    public bool DisplayBackButton { get; init; }

    public bool DisplayHomeButton { get; init; }

    public bool DisplayTopicTitle { get; init; }

    public bool DisplayOrganisationName { get; init; }

    public bool RequiresAuthorisation { get; init; } = true;

    public string? SectionTitle { get; set; }

    public List<ContentComponent> BeforeTitleContent { get; init; } = [];

    public Title? Title { get; init; }

    public string? OrganisationName { get; set; }

    public List<ContentComponent> Content { get; init; } = [];
}
