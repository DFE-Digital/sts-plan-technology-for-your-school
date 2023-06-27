using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class SystemDetails
{
    public string Id { get; init; } = null!;

    public ContentType ContentType { get; init; } = null!;
}
