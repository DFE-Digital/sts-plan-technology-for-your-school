using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RecommendationChunkEntry: TransformableEntry<RecommendationChunkEntry, CmsRecommendationChunkDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Header { get; init; } = null!;
    public List<Entry<ContentComponent>> Content { get; init; } = [];
    public List<QuestionnaireAnswerEntry> Answers { get; init; } = [];
    public CAndSLinkEntry? CSLink { get; init; }

    public RecommendationChunkEntry() : base(entry => new CmsRecommendationChunkDto(entry)) {}
}
