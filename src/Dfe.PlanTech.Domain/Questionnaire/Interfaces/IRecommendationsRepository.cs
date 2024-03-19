using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationsRepository
{
  public Task<SubtopicRecommendationDbEntity?> GetCompleteRecommendationsForSubtopic(string subtopicId, CancellationToken cancellationToken);

  public Task<RecommendationsViewDto?> GetRecommenationsViewDtoForSubtopicAndMaturity(string subtopicId, string maturity, CancellationToken cancellationToken);
}
