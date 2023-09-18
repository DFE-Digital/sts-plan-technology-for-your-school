using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IGetSectionQuery
{
    public Task<Section?> GetSectionById(string sectionId, CancellationToken cancellationToken = default);

    public Task<Section?> GetSectionBySlug(string sectionSlug, CancellationToken cancellationToken = default);
}
