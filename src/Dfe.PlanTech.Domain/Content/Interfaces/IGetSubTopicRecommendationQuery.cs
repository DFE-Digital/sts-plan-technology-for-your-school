using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Queries;

public interface IGetSubTopicRecommendationQuery
{
    public Task<SubTopicRecommendation?> GetSubTopicRecommendation(string subTopicId, CancellationToken cancellationToken = default);

    public Task<RecommendationsViewDto?> GetRecommendationsViewDto(string subTopicId, string maturity, CancellationToken cancellationToken = default);
}
