using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Models;

public class Page : ContentComponent, IPageContent
{
    public string InternalName { get; init; } = null!;

    public string Slug { get; init; } = null!;

    public bool DisplayBackButton { get; init; }

    public bool DisplayHomeButton { get; init; }

    public bool DisplayTopicTitle { get; init; }

    public bool DisplayOrganisationName { get; init; }

    public bool RequiresAuthorisation { get; init; } = true;

    public string? SectionTitle { get; set; }

    public ContentComponent[] BeforeTitleContent { get; init; } = Array.Empty<ContentComponent>();

    public Title? Title { get; init; }

    public string? OrganisationName { get; set; }

    public ContentComponent[] Content { get; init; } = Array.Empty<ContentComponent>();
}
