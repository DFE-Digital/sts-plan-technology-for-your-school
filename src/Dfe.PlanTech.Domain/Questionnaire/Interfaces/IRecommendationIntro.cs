using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Enums;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationIntro
{
    public string Title { get; }

    public Maturity Maturity { get; }
}

public interface IRecommendationIntro<TContentComponent> : IRecommendationIntro
where TContentComponent : IContentComponent
{
    public List<TContentComponent> Content { get; }
}