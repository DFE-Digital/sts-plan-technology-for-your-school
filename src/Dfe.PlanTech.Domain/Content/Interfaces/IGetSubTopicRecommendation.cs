using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Queries;

public interface IGetSubTopicRecommendation
{
    public Task<SubTopicRecommendation> GetSubTopicRecommendation(Section subTopic, CancellationToken cancellationToken = default);
}