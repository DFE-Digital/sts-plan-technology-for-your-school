using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ISubTopicRecommendation { }

public interface ISubTopicRecommendation<TRecommendationIntro, TRecommendationSection, TSection> : ISubTopicRecommendation
where TRecommendationIntro : IRecommendationIntro
where TRecommendationSection : IRecommendationSection<Answer, RecommendationChunk>
where TSection : ISection
{
    public List<TRecommendationIntro> Intros { get; }

    public TRecommendationSection Section { get; }

    public TSection Subtopic { get; }
}