using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IRecommendationChunk<TAnswer, TContentComponent, THeader>
where TAnswer : IAnswer
where TContentComponent : IContentComponentType
where THeader : IHeader
{
    public THeader Header { get; }

    public List<TContentComponent> Content { get; }

    public List<TAnswer> Answers { get; }
}
