using Dfe.PlanTech.Domain.Content.Models.ContentSupport;

namespace Dfe.PlanTech.Domain.Content.Queries;

/// <summary>
/// Retrieves a page from contentful
/// </summary>
public interface IGetContentSupportPageQuery
{
    /// <summary>
    /// Fetches content support page from <see chref="IContentRepository"/> by slug
    /// </summary>
    /// <param name="slug">Slug for the C&S Page</param>
    /// <returns>ContentSupportPage matching slug</returns>
    Task<ContentSupportPage?> GetContentSupportPage(string slug, CancellationToken cancellationToken = default);

}
