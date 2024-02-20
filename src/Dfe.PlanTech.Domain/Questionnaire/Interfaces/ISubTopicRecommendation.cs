using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ISubTopicRecommendation { }

public interface ISubTopicRecommendation<TRecommendationIntro, TRecommendationSection> : ISubTopicRecommendation
where TRecommendationIntro : IRecommendationIntro
where TRecommendationSection : IRecommendationSection<Answer, RecommendationChunk>
{
    public List<TRecommendationIntro> Intros { get; }

    public TRecommendationSection Section { get; }
}