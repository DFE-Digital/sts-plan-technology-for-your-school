using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class RecommendationSectionEntry: TransformableEntry<RecommendationSectionEntry, CmsRecommendationSectionDto>
{
    public string InternalName { get; set; } = null!;
    [NotMapped]
    public List<QuestionnaireAnswerEntry> Answers { get; init; } = [];
    [NotMapped]
    public IEnumerable<RecommendationChunkEntry> Chunks { get; init; } = [];

    protected override Func<RecommendationSectionEntry, CmsRecommendationSectionDto> Constructor => entry => new(entry);
}
