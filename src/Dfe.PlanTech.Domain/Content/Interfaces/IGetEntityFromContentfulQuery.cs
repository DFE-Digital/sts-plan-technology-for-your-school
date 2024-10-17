using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Retrieves entities directly from Contentful
/// </summary>
public interface IGetEntityFromContentfulQuery
{
    Task<TContent?> GetEntityById<TContent>(string contentId, CancellationToken cancellationToken = default)
        where TContent : ContentComponent;
}
