
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
    public GetNavigationQuery(IContentRepository repository) : base(repository)
    {
    }

    public Task<IEnumerable<NavigationLink>> GetNavigationLinks(CancellationToken cancellationToken = default)
        => repository.GetEntities<NavigationLink>(cancellationToken);
}
