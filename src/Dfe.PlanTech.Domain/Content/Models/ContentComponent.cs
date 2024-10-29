using System.Text.Json.Serialization;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public abstract class ContentComponent : IContentComponent
{
    public SystemDetails Sys { get; init; } = null!;

    [JsonConstructor]
    public ContentComponent() { }
}
