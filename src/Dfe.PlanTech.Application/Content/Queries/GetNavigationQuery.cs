
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Application.Content.Queries;

/// <summary>
/// Retrieves Navigation links from the CMS 
/// </summary>
public class GetNavigationQuery : ContentRetriever, IGetNavigationQuery
{
    private readonly ICmsDbContext _db;

    public GetNavigationQuery(ICmsDbContext db, IContentRepository repository) : base(repository)
    {
        _db = db;
    }

    public async Task<IEnumerable<INavigationLink>> GetNavigationLinks(CancellationToken cancellationToken = default)
    {
        var navigationLinks = await _db.ToListAsync(_db.NavigationLink);

        if (navigationLinks.Count > 0) return navigationLinks;

        return await repository.GetEntities<NavigationLink>(cancellationToken);
    }
}
