using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Retrieves sections from contentful
/// </summary>
public interface IGetSectionsQuery
{
    public Task<IEnumerable<Section?>> GetSections(CancellationToken cancellationToken = default);
}
