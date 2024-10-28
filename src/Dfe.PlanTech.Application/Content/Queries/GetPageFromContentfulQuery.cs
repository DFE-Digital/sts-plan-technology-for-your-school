using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Users.Exceptions;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageFromContentfulQuery : IGetPageQuery
{
    private readonly IContentRepository _repository;
    private readonly ILogger<GetPageFromContentfulQuery> _logger;
    private readonly GetPageFromContentfulOptions _options;

    public GetPageFromContentfulQuery(IContentRepository repository, ILogger<GetPageFromContentfulQuery> logger, GetPageFromContentfulOptions options)
    {
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
            var pages = await _repository.GetEntities<Page>(options, cancellationToken);

            var page = pages.FirstOrDefault();

            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching page {slug} from Contentful", slug);
            return null;
        }
    }

    private GetEntitiesOptions CreateGetEntityOptions(string slug) =>
      new(_options.Include, new[] { new ContentQueryEquals() { Field = "fields.slug", Value = slug } });
}
