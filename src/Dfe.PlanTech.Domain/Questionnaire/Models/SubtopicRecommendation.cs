using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class SubtopicRecommendation : ContentComponent, ISubTopicRecommendation<RecommendationIntro, RecommendationSection, Section>
{
    public List<RecommendationIntro> Intros { get; init; } = [];

    public RecommendationSection Section { get; init; } = null!;

    public Section Subtopic { get; init; } = null!;
    
    public RecommendationIntro GetRecommendationByMaturity(Maturity maturity)
    {
        return Intros.FirstOrDefault(intro => intro.Maturity == maturity.ToString()) ?? throw new KeyNotFoundException($"Could not find intro for given maturity:  {maturity}");
    }
    
}