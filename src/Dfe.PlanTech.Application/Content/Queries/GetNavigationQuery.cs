
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

/// <summary>
/// Retrieves Navigation links from the CMS
/// </summary>
public class GetNavigationQuery : ContentRetriever, IGetNavigationQuery
{
    public const string ExceptionMessageContentful = "Error getting navigation links from Contentful";

    public const string ExceptionMessageDatabase = "Error getting navigation links from database";

    private readonly ICmsDbContext _db;
    private readonly ILogger<GetNavigationQuery> _logger;
    private readonly ICmsCache _cache;

    public GetNavigationQuery(ICmsDbContext db, ILogger<GetNavigationQuery> logger, IContentRepository repository, ICmsCache cache) : base(repository)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<INavigationLink>> GetNavigationLinks(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync("NavigationLinks", () => GetFromContentful(cancellationToken)) ?? [];
    }

    private async Task<List<NavigationLinkDbEntity>> GetFromDatabase()
    {
        try
        {
            var navigationLinks = await _db.ToListCachedAsync(_db.NavigationLink);

            return navigationLinks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessageDatabase);
            return new();
        }
    }

    private async Task<IEnumerable<NavigationLink>> GetFromContentful(CancellationToken cancellationToken)
    {
        try
        {
            var navigationLinks = await repository.GetEntities<NavigationLink>(cancellationToken);
            return navigationLinks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessageContentful);
            return [];
        }
    }
}
