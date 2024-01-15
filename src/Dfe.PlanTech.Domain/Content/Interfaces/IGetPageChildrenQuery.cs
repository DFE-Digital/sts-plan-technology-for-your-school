using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IGetPageChildrenQuery
{
  public Task TryLoadChildren(PageDbEntity page, CancellationToken cancellationToken);
}
