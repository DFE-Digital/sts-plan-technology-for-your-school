using Dfe.PlanTech.Application.Persistence.Interfaces;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetPageAuthenticationQuery
{
  private readonly ICmsDbContext _db;

  public GetPageAuthenticationQuery(ICmsDbContext db)
  {
    _db = db;
  }

  public async Task<bool> PageRequiresAuthentication(string slug, CancellationToken cancellationToken)
  {
    var result = await CheckPageAuthenticationFromDb(slug, cancellationToken);

    if (result != null) return result.Value;

    return true;
  }

  private async Task<bool?> CheckPageAuthenticationFromDb(string slug, CancellationToken cancellationToken)
  {
    var page = await _db.FirstOrDefaultAsync(_db.Pages.Where(page => page.Slug == slug).Select(page => new
    {
      page.Id,
      page.RequiresAuthorisation
    }), cancellationToken);

    return page?.RequiresAuthorisation;
  }
}