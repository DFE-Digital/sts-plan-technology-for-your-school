using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationSection { }

public interface IRecommendationSection<TAnswer, TContentComponent, TRecommendationChunk> : IRecommendationSection
where TAnswer : IAnswer
where TContentComponent : IContentComponentType
where TRecommendationChunk : IRecommendationChunk<TAnswer, TContentComponent>
{
    public List<TAnswer> Answers { get; }

    public List<TRecommendationChunk> Chunks { get; }
}
