using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationIntro
{
    public Maturity Maturity { get; }
}

public interface IRecommendationIntro<TTitle, TContentComponent> : IRecommendationIntro
where TTitle : ITitle
where TContentComponent : IContentComponent
{
    public Title Title { get; }

    public List<TContentComponent> Content { get; }
}