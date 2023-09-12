using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class Page : ContentComponent
{
    public string InternalName { get; init; } = null!;

    public string Slug { get; init; } = null!;

    public bool DisplayBackButton { get; init; }

    public bool DisplayHomeButton { get; init; }

    public bool DisplayTopicTitle { get; init; }

    public bool RequiresAuthorisation { get; init; } = true;

    public string? SectionTitle { get; set; }

    public string? Param { get; set; }

    public Title? Title { get; init; }

    public IContentComponent[] Content { get; init; } = Array.Empty<IContentComponent>();
}
