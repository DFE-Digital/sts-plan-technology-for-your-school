using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class Page : ContentComponent
{
    public string InternalName { get; init; } = null!;

    public string Slug { get; init; } = null!;

    public bool DisplayTopicTitle { get; init;}
    
    public string? SectionTitle { get; set; }

    public Title? Title { get; init; }

    public IContentComponent[] Content { get; init; } = Array.Empty<IContentComponent>();
}
