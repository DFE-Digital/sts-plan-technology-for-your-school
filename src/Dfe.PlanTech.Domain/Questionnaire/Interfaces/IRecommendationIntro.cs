using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationIntro
{
    public string Maturity { get; }
}

public interface IRecommendationIntro<TTitle, TContentComponent> : IRecommendationIntro
where TTitle : ITitle
where TContentComponent : IContentComponent
{
    public Title Title { get; }

    public List<TContentComponent> Content { get; }
}