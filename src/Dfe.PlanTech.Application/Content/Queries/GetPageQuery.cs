using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageQuery(GetPageFromContentfulQuery getPageFromContentfulQuery, GetPageFromDbQuery getPageFromDbQuery) : IGetPageQuery
{
    /// <summary>
    /// Fetches page from <see chref="IContentRepository"/> by slug
    /// </summary>
    /// <param name="slug">Slug for the Page</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Page matching slug</returns>
    public async Task<Page?> GetPageBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var page = await getPageFromDbQuery.GetPageBySlug(slug, cancellationToken) ?? await getPageFromContentfulQuery.GetPageBySlug(slug, cancellationToken);

        return page;
    }
}
