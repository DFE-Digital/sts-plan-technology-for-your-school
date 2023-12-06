using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationPageDbEntity : ContentComponentDbEntity, IRecommendationPage<PageDbEntity>
{
    public string InternalName { get; init; } = null!;

    public string DisplayName { get; init; } = null!;

    public Maturity Maturity { get; init; }

    public PageDbEntity Page { get; init; } = null!;
}
