using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RecommendationChunkEntry: TransformableEntry<RecommendationChunkEntry, CmsRecommendationChunkDto>
{
    public string InternalName { get; set; } = null!;
    public string Header { get; init; } = null!;
    public List<ContentfulEntry> Content { get; init; } = [];
    public List<QuestionnaireAnswerEntry> Answers { get; init; } = [];
    public CAndSLinkEntry? CSLink { get; init; }

    protected override Func<RecommendationChunkEntry, CmsRecommendationChunkDto> Constructor => entry => new(entry);
}
