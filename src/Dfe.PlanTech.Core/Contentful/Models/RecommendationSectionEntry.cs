using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RecommendationSectionEntry: TransformableEntry<RecommendationSectionEntry, CmsRecommendationSectionDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    [NotMapped]
    public List<QuestionnaireAnswerEntry> Answers { get; init; } = [];
    [NotMapped]
    public IEnumerable<RecommendationChunkEntry> Chunks { get; init; } = [];

    public RecommendationSectionEntry() : base(entry => new CmsRecommendationSectionDto(entry)) {}
}
