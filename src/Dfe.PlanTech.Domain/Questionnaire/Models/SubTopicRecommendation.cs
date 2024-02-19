using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class SubTopicRecommendation : ContentComponent, ISubTopicRecommendation<RecommendationIntro, RecommendationSection>
{
    public List<RecommendationIntro> RecommendationIntros { get; init; } = [];

    public RecommendationSection RecommendationSection { get; init; } = null!;
}