
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Retrieves a page from contentful
/// </summary>
public interface IGetPageQuery
{
    /// <summary>
    /// Fetches page from <see chref="IContentRepository"/> by slug
    /// </summary>
    /// <param name="slug">Slug for the Page</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Page matching slug</returns>
    public Task<Page?> GetPageBySlug(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches page from <see chref="IContentRepository"/> by id
    /// </summary>
    /// <param name="pageId">Content Component ID of the page in Contentful</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Page matching Id</returns>
    public Task<Page?> GetPageById(string pageId, CancellationToken cancellationToken = default);
}
