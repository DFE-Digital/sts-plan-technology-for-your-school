using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageQuery(GetPageFromContentfulQuery getPageFromContentfulQuery, GetPageFromDbQuery getPageFromDbQuery, IDistributedCache cache) : IGetPageQuery
{
    /// <summary>
    /// Fetches page from <see chref="IContentRepository"/> by slug
    /// </summary>
    /// <param name="slug">Slug for the Page</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Page matching slug</returns>
    public async Task<Page?> GetPageBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var page = await cache.GetOrCreateAsync($"Page:{slug}", () => GetPage(slug, cancellationToken));

        return page;
    }

    private async Task<Page?> GetPage(string slug, CancellationToken cancellationToken)
    => await getPageFromDbQuery.GetPageBySlug(slug, cancellationToken) ?? await getPageFromContentfulQuery.GetPageBySlug(slug, cancellationToken);
}
