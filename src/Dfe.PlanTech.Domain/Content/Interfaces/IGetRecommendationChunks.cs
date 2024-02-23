using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Queries;

public interface IGetRecommendationChunks
{
    public Task<IEnumerable<RecommendationChunk>> GetRecommendationChunksFromAnswers(Section subTopic, IEnumerable<Answer> answers, CancellationToken cancellationToken = default);
}