using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ISubTopicRecommendation { }

public interface ISubTopicRecommendation<TAnswer, TContentComponent, TRecommendationChunk, TRecommendationIntro, TRecommendationSection, TSection> : ISubTopicRecommendation
where TAnswer : IAnswer
where TContentComponent : IContentComponentType
where TRecommendationChunk : IRecommendationChunk<TAnswer, TContentComponent>
where TRecommendationIntro : IRecommendationIntro
where TRecommendationSection : IRecommendationSection<TAnswer, TContentComponent, TRecommendationChunk>
where TSection : ISection
{
    public List<TRecommendationIntro> Intros { get; }

    public TRecommendationSection Section { get; }

    public TSection Subtopic { get; }
}
