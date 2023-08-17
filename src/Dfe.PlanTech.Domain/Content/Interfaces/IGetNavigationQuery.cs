using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IGetNavigationQuery
{
    Task<IEnumerable<NavigationLink>> GetNavigationLinks(CancellationToken cancellationToken = default);
}