using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ISubTopicRecommendation<TRecommendationIntro, TRecommendationSection>
where TRecommendationIntro : IRecommendationIntro
where TRecommendationSection : IRecommendationSection<Answer, RecommendationChunk>
{
    public List<TRecommendationIntro> RecommendationIntros { get; }

    public TRecommendationSection RecommendationSection { get; }
}