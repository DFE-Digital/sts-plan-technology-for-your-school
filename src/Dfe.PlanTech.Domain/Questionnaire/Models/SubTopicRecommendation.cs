using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class SubTopicRecommendation : ContentComponent, ISubTopicRecommendation<RecommendationIntro, RecommendationSection>
{
    public List<RecommendationIntro> Intros { get; init; } = [];

    public RecommendationSection Section { get; init; } = null!;
}