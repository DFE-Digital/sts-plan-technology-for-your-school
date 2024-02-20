using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationIntro
{
    public string Maturity { get; }
}

public interface IRecommendationIntro<THeader, TContentComponent> : IRecommendationIntro
where THeader : IHeader
where TContentComponent : IContentComponent
{
    public Header Header { get; }

    public List<TContentComponent> Content { get; }
}