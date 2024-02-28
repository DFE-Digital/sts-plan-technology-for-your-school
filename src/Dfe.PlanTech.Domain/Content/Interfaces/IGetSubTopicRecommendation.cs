using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Queries;

public interface IGetSubTopicRecommendation
{
    public Task<SubtopicRecommendation> GetSubTopicRecommendation(string subTopic, CancellationToken cancellationToken = default);
}