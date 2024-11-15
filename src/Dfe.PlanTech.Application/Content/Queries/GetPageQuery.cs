using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageQuery : IGetPageQuery
{
    private readonly ICmsCache _cache;
    private readonly IContentRepository _repository;
    private readonly ILogger<GetPageQuery> _logger;
    private readonly GetPageFromContentfulOptions _options;

    public GetPageQuery(ICmsCache cache, IContentRepository repository, ILogger<GetPageQuery> logger, GetPageFromContentfulOptions options)
    {
        _cache = cache;
        _repository = repository;
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Retrieves the page for the given slug from Contentful
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="ContentfulDataUnavailableException"></exception>
    public async Task<Page?> GetPageBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var options = CreateGetEntityOptions(slug);

        return await FetchFromContentful(slug, options, cancellationToken);
    }

    /// <summary>
    /// Get page by slug + only return specific fields
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="fieldsToReturn"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ContentfulDataUnavailableException"></exception>

    public async Task<Page?> GetPageBySlug(string slug, IEnumerable<string> fieldsToReturn, CancellationToken cancellationToken = default)
    {
        var options = CreateGetEntityOptions(slug);
        options.Select = fieldsToReturn;

        return await FetchFromContentful(slug, options, cancellationToken);
    }

    /// <exception cref="ContentfulDataUnavailableException"></exception>
    private async Task<Page?> FetchFromContentful(string slug, GetEntitiesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var pages = await _cache.GetOrCreateAsync($"Page:{slug}", () => _repository.GetEntities<Page?>(options, cancellationToken)) ?? [];

            var page = pages.FirstOrDefault();

            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching page {slug} from Contentful", slug);
            throw new ContentfulDataUnavailableException($"Could not retrieve page with slug {slug}", ex);
        }
    }

    private GetEntitiesOptions CreateGetEntityOptions(string slug) =>
      new(_options.Include, new[] { new ContentQueryEquals() { Field = "fields.slug", Value = slug } });
}
