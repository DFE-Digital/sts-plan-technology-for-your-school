
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

    public GetNavigationQuery(ICmsDbContext db, ILogger<GetNavigationQuery> logger, IContentRepository repository) : base(repository)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<INavigationLink>> GetNavigationLinks(CancellationToken cancellationToken = default)
    {
        var navigationLinks = await GetFromDatabase();

        if (navigationLinks.Count > 0)
            return navigationLinks;

        return await GetFromContentful(cancellationToken);
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
