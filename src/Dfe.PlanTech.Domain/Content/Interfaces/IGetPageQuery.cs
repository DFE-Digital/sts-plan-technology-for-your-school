
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Queries;

/// <summary>
/// Retrieves a page from contentful
/// </summary>
public interface IGetPageQuery
{
    /// <summary>
    /// Fetches page from <see chref="IContentRepository"/> by slug
    /// </summary>
    /// <param name="slug">Slug for the Page</param>
    /// <returns>Page matching slug</returns>
    public Task<Page?> GetPageBySlug(string slug, CancellationToken cancellationToken = default);
}
