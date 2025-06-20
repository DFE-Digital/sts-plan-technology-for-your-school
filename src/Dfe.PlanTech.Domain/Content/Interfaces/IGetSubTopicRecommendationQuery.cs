using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IGetSubTopicRecommendationQuery
{
    public Task<SubtopicRecommendation?> GetSubTopicRecommendation(string subtopicId, CancellationToken cancellationToken = default);

    public Task<RecommendationsViewDto?> GetRecommendationsViewDto(string subtopicId, string maturity, CancellationToken cancellationToken = default);
}
