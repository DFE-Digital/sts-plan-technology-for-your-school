using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IContentComponent : IContentComponentType
{
    public SystemDetails Sys { get; }
}
