using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationSection : ContentComponent, IRecommendationSection<Answer, RecommendationChunk>
{
    public List<Answer> Answers { get; init; } = [];

    public List<RecommendationChunk> Chunks { get; init; } = [];
    
    public List<RecommendationChunk> GetRecommendationChunksByAnswerIds(IEnumerable<string> answerIds)
    {
        return Chunks
            .Where(chunk => chunk.Answers.Any(chunkAnswer => answerIds.Contains(chunkAnswer.Sys.Id)))
            .Distinct()
            .ToList();
    }
}