using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public class EntryBase : IContentComponentType
{
    public SystemDetails Sys { get; }
}
