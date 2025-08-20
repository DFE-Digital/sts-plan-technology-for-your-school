using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RecommendationSectionEntry: ContentfulEntry
{
    public string InternalName { get; set; } = null!;

    [NotMapped]
    public List<QuestionnaireAnswerEntry> Answers { get; init; } = [];

    [NotMapped]
    public IEnumerable<RecommendationChunkEntry> Chunks { get; init; } = [];

    public List<RecommendationChunkEntry> GetRecommendationChunksByAnswerIds(IEnumerable<string> answerIds)
    {
        return Chunks
            .Where(chunk => chunk.Answers.Exists(chunkAnswer => answerIds.Contains(chunkAnswer.Id)))
            .DistinctBy(chunk => chunk.Id)
            .ToList();
    }
}
