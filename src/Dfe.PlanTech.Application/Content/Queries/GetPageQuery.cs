using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageQuery : IGetPageQuery
{
    private readonly ILogger<GetPageQuery> _logger;
    private readonly IQuestionnaireCacher _cacher;
    private readonly IGetPageQuery _getPageFromDbQuery;
    private readonly IGetPageQuery _getPageFromContentfulQuery;

    public GetPageQuery(GetPageFromContentfulQuery getPageFromContentfulQuery, GetPageFromDbQuery getPageFromDbQuery, ILogger<GetPageQuery> logger, IQuestionnaireCacher cacher)
    {
        _cacher = cacher;
        _logger = logger;
        _getPageFromDbQuery = getPageFromDbQuery;
        _getPageFromContentfulQuery = getPageFromContentfulQuery;
    }

    /// <summary>
    /// Fetches page from <see chref="IContentRepository"/> by slug
    /// </summary>
    /// <param name="slug">Slug for the Page</param>
    /// <returns>Page matching slug</returns>
    public async Task<Page?> GetPageBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var page = await _getPageFromDbQuery.GetPageBySlug(slug, cancellationToken) ??
                    await _getPageFromContentfulQuery.GetPageBySlug(slug, cancellationToken);

        if (page != null)
        {
            UpdateSectionTitle(page);
        }

        return page;
    }

    private void UpdateSectionTitle(Page page)
    {
        if (page.DisplayTopicTitle)
        {
            var cached = _cacher.Cached!;
            page.SectionTitle = cached.CurrentSectionTitle;
        }
    }
}