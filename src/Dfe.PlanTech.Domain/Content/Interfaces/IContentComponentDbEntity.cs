using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IContentComponentDbEntity : IContentComponentType, IHasId<string>
{
    public List<PageDbEntity> BeforeTitleContentPages { get; set; }

    public List<PageDbEntity> ContentPages { get; set; }
}