using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IContentComponent : IContentComponentType
{
    public SystemDetails Sys { get; }
}

public interface IContentComponentType
{

}


public interface IContentComponentDbEntity : IContentComponentType
{
    public string Id { get; set; }
}