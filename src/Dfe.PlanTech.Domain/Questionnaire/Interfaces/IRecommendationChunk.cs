using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationChunk
{
    public string Title { get; }
}

public interface IRecommendationChunk<THeader, TContentComponent, TAnswer> : IRecommendationChunk
where THeader : IHeader
where TContentComponent : IContentComponent
where TAnswer : IAnswer
{
    public THeader Header { get; }

    public List<TContentComponent> Content { get; }

    public List<TAnswer> Answers { get; }
}