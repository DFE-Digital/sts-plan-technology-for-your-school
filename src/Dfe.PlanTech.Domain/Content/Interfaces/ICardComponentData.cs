using Contentful.Core.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface ICardComponentData
{
    public string? Title { get; }
    public string? Description { get; }
    public string? Meta { get; }
    public Asset? Image { get; }
    public string? ImageAlt { get; }
    public string? Uri { get; }
}
