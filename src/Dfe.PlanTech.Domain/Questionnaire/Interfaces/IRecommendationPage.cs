using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Enums;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationPage
{
    public string InternalName { get; }

    public string DisplayName { get; }

    public Maturity Maturity { get; }
}

public interface IRecommendationPage<TPage> : IRecommendationPage
where TPage : IPage
{
    public TPage Page { get; }
}
