using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class SubtopicRecommendation : ContentComponent, ISubTopicRecommendation<Answer, ContentComponent, Header, RecommendationChunk, RecommendationIntro, RecommendationSection, Section>
{
    public List<RecommendationIntro> Intros { get; init; } = [];

    public RecommendationSection Section { get; init; } = null!;

    public Section Subtopic { get; init; } = null!;

    public RecommendationIntro? GetRecommendationByMaturity(string maturity)
    => Intros.FirstOrDefault(intro => intro.Maturity == maturity);

    public RecommendationIntro? GetRecommendationByMaturity(Maturity maturity)
    => Intros.FirstOrDefault(intro => intro.Maturity == maturity.ToString());
}
