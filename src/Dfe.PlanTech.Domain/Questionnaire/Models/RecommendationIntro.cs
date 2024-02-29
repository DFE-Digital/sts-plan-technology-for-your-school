using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationIntro : ContentComponent, IRecommendationIntro<Header, ContentComponent>
{
    public Header Header { get; init; } = null!;

    public string Maturity { get; init; } = null!;

    public List<ContentComponent> Content { get; init; } = [];
}