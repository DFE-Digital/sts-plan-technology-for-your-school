using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationChunk<TAnswer, TContentComponent>
where TAnswer : IAnswer
where TContentComponent : IContentComponentType
{
    public string Header { get; }

    public List<TContentComponent> Content { get; }

    public List<TAnswer> Answers { get; }
}
