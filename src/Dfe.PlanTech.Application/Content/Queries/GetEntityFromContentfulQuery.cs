using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

/// <summary>
/// Retrieves entities directly from Contentful
/// This is only intended for use with Preview routes, as the results are not cached.
/// </summary>
public class GetEntityFromContentfulQuery : ContentRetriever, IGetEntityFromContentfulQuery
{
    public const string ExceptionMessageEntityContentful = "Error fetching Entity from Contentful";
    public const string ExceptionMessageEntitiesContentful = "Error fetching Entities from Contentful";

    private readonly ILogger<GetEntityFromContentfulQuery> _logger;
    private readonly ICmsCache _cache;

    public GetEntityFromContentfulQuery(ILogger<GetEntityFromContentfulQuery> logger, IContentRepository repository, ICmsCache cache) : base(repository)
    {
        _logger = logger;
        _cache = cache;
    }

    public async Task<TContent?> GetEntityById<TContent>(string contentId, CancellationToken cancellationToken = default)
        where TContent : ContentComponent
    {
        try
        {
            return await _cache.GetOrCreateAsync($"Entity:{contentId}",
                () => repository.GetEntityById<TContent>(contentId, cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessageEntityContentful);
            return null;
        }
    }

    public async Task<IEnumerable<TContent>?> GetEntities<TContent>(int depth = 2, CancellationToken cancellationToken = default)
        where TContent : ContentComponent
    {
        try
        {
            var options = new GetEntitiesOptions(include: depth);
            return await _cache.GetOrCreateAsync($"Entities:{typeof(TContent).Name}:(depth {depth})",
                () => repository.GetEntities<TContent>(options, cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessageEntitiesContentful);
            return null;
        }
    }
}
