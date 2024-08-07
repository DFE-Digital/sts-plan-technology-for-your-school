using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationIntro
{
    public string? Maturity { get; }
}

public interface IRecommendationIntro<THeader, TContentComponent> : IRecommendationIntro
where THeader : IHeader
where TContentComponent : IContentComponentType
{
    public THeader Header { get; }

    public List<TContentComponent> Content { get; }
}
