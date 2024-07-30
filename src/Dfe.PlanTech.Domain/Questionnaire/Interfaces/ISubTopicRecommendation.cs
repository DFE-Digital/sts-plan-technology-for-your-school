using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ISubTopicRecommendation { }

public interface ISubTopicRecommendation<TAnswer, TContentComponent, THeader, TRecommendationChunk, TRecommendationIntro, TRecommendationSection, TSection> : ISubTopicRecommendation
where TAnswer : IAnswer
where TContentComponent : IContentComponentType
where THeader : IHeader
where TRecommendationChunk : IRecommendationChunk<TAnswer, TContentComponent, THeader>
where TRecommendationIntro : IRecommendationIntro
where TRecommendationSection : IRecommendationSection<TAnswer, TContentComponent, THeader, TRecommendationChunk>
where TSection : ISection
{
    public List<TRecommendationIntro> Intros { get; }

    public TRecommendationSection Section { get; }

    public TSection Subtopic { get; }
}
