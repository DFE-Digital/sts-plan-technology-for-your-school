using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class SubtopicRecommendationEntry : TransformableEntry<SubtopicRecommendationEntry, CmsSubtopicRecommendationDto>
{
    public string InternalName { get; set; } = null!;
    public List<RecommendationIntroEntry> Intros { get; init; } = [];
    public RecommendationSectionEntry Section { get; init; } = null!;
    public QuestionnaireSectionEntry Subtopic { get; init; } = null!;

    public SubtopicRecommendationEntry() : base(entry => new CmsSubtopicRecommendationDto(entry)) { }
}
