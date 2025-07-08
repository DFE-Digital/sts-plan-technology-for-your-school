using Contentful.Core.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class SubtopicRecommendationEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public List<RecommendationIntroEntry> Intros { get; init; } = [];
    public RecommendationSectionEntry Section { get; init; } = null!;
    public QuestionnaireSectionEntry Subtopic { get; init; } = null!;
}
