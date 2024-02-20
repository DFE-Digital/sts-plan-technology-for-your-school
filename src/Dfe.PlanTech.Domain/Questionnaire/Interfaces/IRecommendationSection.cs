using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationSection<TAnswer, TRecommendationChunk>
where TAnswer : IAnswer
where TRecommendationChunk : IRecommendationChunk<Title, Header, ContentComponent, Answer>
{
    public List<TAnswer> Answers { get; }

    public List<TRecommendationChunk> Chunks { get; }
}