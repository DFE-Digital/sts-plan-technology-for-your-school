using AutoMapper;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageFromDbQuery : IGetPageQuery
{
  private readonly ICmsDbContext _db;
  private readonly ILogger<GetPageFromDbQuery> _logger;
  private readonly IMapper _mapperConfiguration;
  private readonly IEnumerable<IGetPageChildrenQuery> _getPageChildrenQueries;

  public GetPageFromDbQuery(ICmsDbContext db, ILogger<GetPageFromDbQuery> logger, IMapper mapperConfiguration, IEnumerable<IGetPageChildrenQuery> getPageChildrenQueries)
  {
    _db = db;
    _logger = logger;
    _mapperConfiguration = mapperConfiguration;
    _getPageChildrenQueries = getPageChildrenQueries;
  }

  /// <summary>
  /// Fetches page from <see cref="ICmsDbContext"/> by slug
  /// </summary>
  /// <param name="slug">Slug for the Page</param>
  /// <returns>Page matching slug</returns>
  public async Task<Page?> GetPageBySlug(string slug, CancellationToken cancellationToken = default)
  {
    try
    {
      var page = await RetrievePageFromDatabase(slug, cancellationToken);

      if (page == null) return null;

      return _mapperConfiguration.Map<PageDbEntity, Page>(page);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error fetching {page} from database", slug);
      return null;
    }
  }

  private async Task<PageDbEntity?> RetrievePageFromDatabase(string slug, CancellationToken cancellationToken)
  {
    var page = await _db.GetPageBySlug(slug, cancellationToken);

    if (!IsValidPage(page, slug))
    {
      _logger.LogError("Retrieved page {slug} is invalid", slug);
      return null;
    }

    await LoadPageChildrenFromDatabase(page, cancellationToken);

    return page;
  }

  private async Task LoadPageChildrenFromDatabase(PageDbEntity? page, CancellationToken cancellationToken)
  {
    foreach (var query in _getPageChildrenQueries)
    {
      await query.TryLoadChildren(page!, cancellationToken);
    }
  }

  /// <summary>
  /// Checks the retrieved Page from the DB to ensure it is valid (e.g. not null, has content). If not, logs message and returns false.
  /// </summary>
  /// <param name="page"></param>
  /// <param name="slug"></param>
  /// <returns></returns>
  private bool IsValidPage(PageDbEntity? page, string slug)
  {
    if (page == null)
    {
      _logger.LogInformation("Could not find page {slug} in DB - checking Contentful", slug);
      return false;
    }

    if (page.Content == null || page.Content.Count == 0)
    {
      _logger.LogWarning("Page {slug} has no 'Content' in DB - checking Contentful", slug);
      return false;
    }

    return true;
  }

}