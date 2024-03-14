using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationsRepository
{
  public Task<SubtopicRecommendationDbEntity?> GetRecommendationsForSubtopic(string subtopicId, CancellationToken cancellationToken);
}
