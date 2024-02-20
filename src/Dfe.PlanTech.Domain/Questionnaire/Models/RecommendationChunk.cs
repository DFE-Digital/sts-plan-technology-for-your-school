using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationChunk : ContentComponent, IRecommendationChunk<Title, Header, ContentComponent, Answer>
{
    public Title Title { get; init; } = null!;

    public Header Header { get; init; } = null!;

    public List<ContentComponent> Content { get; init; } = [];

    public List<Answer> Answers { get; init; } = [];
}
