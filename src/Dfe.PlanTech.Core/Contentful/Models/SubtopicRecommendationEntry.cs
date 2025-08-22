namespace Dfe.PlanTech.Core.Contentful.Models;

public class SubtopicRecommendationEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public List<RecommendationIntroEntry> Intros { get; init; } = [];
    public RecommendationSectionEntry Section { get; init; } = null!;
    public QuestionnaireSectionEntry Subtopic { get; init; } = null!;

    public RecommendationIntroEntry? GetRecommendationByMaturity(string? maturity) =>
        Intros.FirstOrDefault(intro => !string.IsNullOrWhiteSpace(intro.Maturity) && intro.Maturity.Equals(maturity));
}
