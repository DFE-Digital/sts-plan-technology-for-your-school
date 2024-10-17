using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IGetSectionQuery
{
    public Task<Section?> GetSectionBySlug(string sectionSlug, CancellationToken cancellationToken = default);

    public Task<Section?> GetSectionForQuestion(string questionId, CancellationToken cancellationToken = default);
}
