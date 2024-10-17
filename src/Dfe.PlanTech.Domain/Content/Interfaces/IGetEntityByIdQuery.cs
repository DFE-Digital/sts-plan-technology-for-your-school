using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Retrieves entities from the CMS
/// </summary>
public interface IGetEntityByIdQuery
{
    Task<Question?> GetQuestion(string questionId, CancellationToken cancellationToken = default);
}
