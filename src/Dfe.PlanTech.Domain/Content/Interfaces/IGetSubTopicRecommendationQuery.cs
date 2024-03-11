using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Queries;

public interface IGetSubTopicRecommendationQuery
{
    public Task<SubTopicRecommendation?> GetSubTopicRecommendation(string subTopicId, CancellationToken cancellationToken = default);
}
