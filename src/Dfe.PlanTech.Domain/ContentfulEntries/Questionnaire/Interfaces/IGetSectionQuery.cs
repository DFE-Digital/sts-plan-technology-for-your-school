using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;

public interface IGetSectionQuery
{
    public Task<Section?> GetSectionBySlug(string sectionSlug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all sections from contentful
    /// </summary>
    public Task<IEnumerable<Section?>> GetAllSections(CancellationToken cancellationToken = default);
}
