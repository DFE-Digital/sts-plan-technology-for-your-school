using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationPage : ContentComponent, IRecommendationPage<Page>
{
    public string InternalName { get; init; } = null!;

    public string DisplayName { get; init; } = null!;

    public Maturity Maturity { get; init; }

    public Page Page { get; init; } = null!;
}
