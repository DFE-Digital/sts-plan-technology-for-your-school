using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationIntro : ContentComponent, IRecommendationIntro<Title, ContentComponent>
{
    public Title Title { get; init; } = null!;

    public string Maturity { get; init; } = null!;

    public List<ContentComponent> Content { get; init; } = [];
}