namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface ICard
{
    string? Title { get; }
    string? Description { get; }
    string? Meta { get; }
    string? URI { get; }
}
