using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsRecommendationSectionDto : CmsEntryDto
{
    public string Id { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public List<CmsQuestionnaireAnswerDto> Answers { get; init; } = [];
    public IEnumerable<CmsRecommendationChunkDto> Chunks { get; init; } = [];

    public CmsRecommendationSectionDto(RecommendationSectionEntry recommendationSectionEntry)
    {
        Id = recommendationSectionEntry.Id;
        InternalName = recommendationSectionEntry.InternalName;
        Answers = recommendationSectionEntry.Answers.Select(a => a.AsDto()).ToList();
        Chunks = recommendationSectionEntry.Chunks.Select(c => c.AsDto()).ToList();
    }

    public List<CmsRecommendationChunkDto> GetRecommendationChunksByAnswerIds(IEnumerable<string> answerIds)
    {
        return Chunks
            .Where(chunk => chunk.Answers.Exists(chunkAnswer => answerIds.Contains(chunkAnswer.Id)))
            .DistinctBy(chunk => chunk.Id)
            .ToList();
    }
}
