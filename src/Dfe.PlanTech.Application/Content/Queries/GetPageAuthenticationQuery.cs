using Dfe.PlanTech.Application.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageAuthenticationQuery
{
  private readonly ICmsDbContext _db;
  private readonly GetPageFromContentfulQuery _getPageFromContentfulQuery;
  private readonly ILogger<GetPageAuthenticationQuery> _logger;

  public GetPageAuthenticationQuery(ICmsDbContext db, GetPageFromContentfulQuery getPageFromContentfulQuery, ILogger<GetPageAuthenticationQuery> logger)
  {
    _db = db;
    _getPageFromContentfulQuery = getPageFromContentfulQuery;
    _logger = logger;
  }

  public async Task<bool> PageRequiresAuthentication(string slug, CancellationToken cancellationToken)
  {
    var result = await CheckPageAuthenticationFromDb(slug, cancellationToken) ??
                await CheckPageAuthenticationFromContentful(slug, cancellationToken);

    return result ?? false;
  }

  private async Task<bool?> CheckPageAuthenticationFromDb(string slug, CancellationToken cancellationToken)
  {
    try
    {
      var page = await _db.FirstOrDefaultAsync(_db.Pages.Where(page => page.Slug == slug).Select(page => new
      {
        page.Id,
        page.RequiresAuthorisation
      }), cancellationToken);

      return page?.RequiresAuthorisation;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error checking page authentication in DB for {slug}", slug);
      return null;
    }
  }

  private async Task<bool?> CheckPageAuthenticationFromContentful(string slug, CancellationToken cancellationToken)
  {
    try
    {
      var result = await _getPageFromContentfulQuery.GetPageBySlug(slug, new[] { "fields.requiresAuthorisation" }, cancellationToken);

      return result?.RequiresAuthorisation;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error checking page authentication in Contentful for {slug}", slug);
      return null;
    }
  }
}