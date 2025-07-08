namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Retrieves Navigation links from the CMS 
/// </summary>
public interface IGetNavigationQuery
{
    /// <summary>
    /// Retrieve links
    /// </summary>
    /// <returns>Found navigation links</returns>
    Task<IEnumerable<INavigationLink>> GetNavigationLinks(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve link by id
    /// </summary>
    /// <returns>Found navigation link</returns>
    Task<INavigationLink?> GetLinkById(string contentId, CancellationToken cancellationToken = default);
}
