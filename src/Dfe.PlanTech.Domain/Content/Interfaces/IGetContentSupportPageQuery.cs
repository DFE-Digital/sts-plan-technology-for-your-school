using Dfe.PlanTech.Domain.Content.Models.ContentSupport;

namespace Dfe.PlanTech.Domain.Content.Queries;

/// <summary>
/// Retrieves a page from contentful
/// </summary>
public interface IGetContentSupportPageQuery
{
    /// <summary>
    /// Fetches page from <see chref="IContentRepository"/> by slug
    /// </summary>
    /// <param name="slug">Slug for the Page</param>
    /// <returns>Page matching slug</returns>
    public Task<ContentSupportPage?> GetContentSupportPageBySlug(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<ContentSupportPage>> GetContentSupportPages(CancellationToken cancellationToken = default);
}
