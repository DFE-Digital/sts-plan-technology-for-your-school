using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageFromContentfulQuery : IGetPageQuery
{
  private readonly IContentRepository _repository;
  private readonly ILogger<GetPageAuthenticationQuery> _logger;
  private readonly GetPageFromContentfulOptions _options;

  public GetPageFromContentfulQuery(IContentRepository repository, ILogger<GetPageAuthenticationQuery> logger, GetPageFromContentfulOptions options)
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
    try
    {
      var options = CreateGetEntityOptions(slug);
      var pages = await _repository.GetEntities<Page>(options, cancellationToken);

      var page = pages.FirstOrDefault() ?? throw new KeyNotFoundException($"Could not find page with slug {slug}");

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