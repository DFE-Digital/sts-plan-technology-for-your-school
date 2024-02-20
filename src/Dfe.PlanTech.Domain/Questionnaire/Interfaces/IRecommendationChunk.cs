using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationChunk<TTitle, THeader, TContentComponent, TAnswer>
where TTitle : ITitle
where THeader : IHeader
where TContentComponent : IContentComponent
where TAnswer : IAnswer
{
    public TTitle Title { get; }

    public THeader Header { get; }

    public List<TContentComponent> Content { get; }

    public List<TAnswer> Answers { get; }
}