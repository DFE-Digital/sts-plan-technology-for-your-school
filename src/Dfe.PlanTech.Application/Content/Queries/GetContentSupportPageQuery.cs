using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetContentSupportPageQuery : IGetContentSupportPageQuery
{
    private readonly ICmsCache _cache;
    private readonly IContentRepository _repository;
    private readonly ILogger<GetContentSupportPageQuery> _logger;
    private readonly GetPageFromContentfulOptions _options;

    public GetContentSupportPageQuery(ICmsCache cache, IContentRepository repository, ILogger<GetContentSupportPageQuery> logger, GetPageFromContentfulOptions options)
    {
        _cache = cache;
        _repository = repository;
        _logger = logger;
        _options = options;
    }

    /// <exception cref="ContentfulDataUnavailableException"></exception>
    public async Task<ContentSupportPage?> GetContentSupportPageBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var options = CreateGetEntityOptions(slug);

        var res =  await FetchFromContentful(slug, options, cancellationToken);
        return res;
    }

    public async Task<ContentSupportPage?> GetContentSupportPageBySlug(string slug, IEnumerable<string> fieldsToReturn, CancellationToken cancellationToken = default)
    {

        var options = CreateGetEntityOptions(slug);
        options.Select = fieldsToReturn;

        return await FetchFromContentful(slug, options, cancellationToken);
    }

    /// <exception cref="ContentfulDataUnavailableException"></exception>
    private async Task<ContentSupportPage?> FetchFromContentful(string slug, GetEntitiesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var pages = await _cache.GetOrCreateAsync($"ContentSupportPage:{slug}", () => _repository.GetEntities<ContentSupportPage>(options, cancellationToken)) ?? [];

            var page = pages.FirstOrDefault();

            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching content support page {slug} from Contentful", slug);
            throw new ContentfulDataUnavailableException($"Could not retrieve content support page with slug {slug}", ex);
        }
    }


    public async Task<IEnumerable<ContentSupportPage>> GetContentSupportPages(CancellationToken cancellationToken = default)
    {
        try
        {
            var contentSupportPages = await _cache.GetOrCreateAsync("ContentSupportPages", () => _repository.GetEntities<ContentSupportPage>(cancellationToken)) ?? [];
            return contentSupportPages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching content support pages from Contentful");
            return [];
        }
    }

    private GetEntitiesOptions CreateGetEntityOptions(string slug) =>
      new(_options.Include, new[] { new ContentQueryEquals() { Field = "fields.Slug", Value = slug } });
}