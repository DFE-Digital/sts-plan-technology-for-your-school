using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationSection : ContentComponent, IRecommendationSection<QuestionnaireAnswerEntry, ContentComponent, RecommendationChunk>
{
    [NotMapped]
    public List<QuestionnaireAnswerEntry> Answers { get; init; } = [];

    [NotMapped]
    public List<RecommendationChunk> Chunks { get; init; } = [];

    public List<RecommendationChunk> GetRecommendationChunksByAnswerIds(IEnumerable<string> answerIds)
    {
        return Chunks
            .Where(chunk => chunk.Answers.Exists(chunkAnswer => answerIds.Contains(chunkAnswer.Sys.Id)))
            .Distinct()
            .ToList();
    }
}
