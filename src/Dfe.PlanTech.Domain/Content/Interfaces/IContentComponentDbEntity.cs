using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IContentComponentDbEntity : IContentComponentType
{
    public string Id { get; set; }

    public List<PageDbEntity> BeforeTitleContentPages { get; set; }

    public List<PageDbEntity> ContentPages { get; set; }
}
