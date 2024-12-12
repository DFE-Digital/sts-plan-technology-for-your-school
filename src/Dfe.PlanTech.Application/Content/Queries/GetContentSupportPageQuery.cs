using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
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

    private GetEntitiesOptions CreateGetEntityOptions(string slug) => new(10, [new ContentQueryEquals() { Field = "fields.Slug", Value = slug }]);

    public async Task<ContentSupportPage?> GetContentSupportPage(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var pages = await _cache.GetOrCreateAsync($"ContentSupportPage:{slug}", () => _repository.GetEntities<ContentSupportPage>(CreateGetEntityOptions(slug), cancellationToken)) ?? [];

            var page = pages.FirstOrDefault();

            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching content support page {slug} from Contentful", slug);
            throw new ContentfulDataUnavailableException($"Could not retrieve content support page with slug {slug}", ex);
        }

    }
}
