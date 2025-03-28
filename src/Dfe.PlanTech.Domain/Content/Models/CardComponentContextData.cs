using Contentful.Core.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class CardComponentContextData : ContentComponent, IContentComponent, ICardComponentData
{
    public string InternalName { get; init; } = null!;
    public string? Title { get; init; } = null!;

    public string? Meta { get; init; } = null!;

    public Asset? Image { get; init; } = null!;

    public string? ImageAlt { get; init; } = null!;

    public string? Uri { get; init; } = null!;
}
