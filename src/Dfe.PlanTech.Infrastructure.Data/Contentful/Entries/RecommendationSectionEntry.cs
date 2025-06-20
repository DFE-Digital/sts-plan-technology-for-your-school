using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class RecommendationSectionEntry : ContentfulEntry
    {
        [NotMapped]
        public List<AnswerEntry> Answers { get; init; } = [];

        [NotMapped]
        public List<RecommendationChunkEntry> Chunks { get; init; } = [];

        public List<RecommendationChunkEntry> GetRecommendationChunksByAnswerIds(IEnumerable<string> answerIds)
        {
            return Chunks
                .Where(chunk => chunk.Answers.Exists(chunkAnswer => answerIds.Contains(chunkAnswer.Sys.Id)))
                .Distinct()
                .ToList();
        }
    }
}
