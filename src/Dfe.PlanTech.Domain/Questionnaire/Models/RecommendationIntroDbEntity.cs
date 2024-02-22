using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationIntroDbEntity : ContentComponentDbEntity, IRecommendationIntro<HeaderDbEntity, ContentComponentDbEntity>
{
    public HeaderDbEntity Header { get; init; } = null!;

    public string Maturity { get; init; } = null!;

    public List<ContentComponentDbEntity> Content { get; init; } = [];
}