using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Retrieves Navigation links from the CMS 
/// </summary>
public interface IGetNavigationQuery
{
    /// <summary>
    /// Retrieve links
    /// </summary>
    /// <returns>Fonud navigation links</returns>
    Task<IEnumerable<NavigationLink>> GetNavigationLinks(CancellationToken cancellationToken = default);
}