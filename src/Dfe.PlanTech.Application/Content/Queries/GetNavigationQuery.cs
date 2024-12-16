
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

    private readonly ILogger<GetNavigationQuery> _logger;

    public GetNavigationQuery(ILogger<GetNavigationQuery> logger, IContentRepository repository) : base(repository)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<INavigationLink>> GetNavigationLinks(CancellationToken cancellationToken = default)
    {
        try
        {
            return await repository.GetEntities<NavigationLink>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessageContentful);
            return [];
        }
    }

    public async Task<INavigationLink?> GetLinkById(string contentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await repository.GetEntityById<NavigationLink?>(contentId, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessageContentful);
            return null;
        }
    }
}
