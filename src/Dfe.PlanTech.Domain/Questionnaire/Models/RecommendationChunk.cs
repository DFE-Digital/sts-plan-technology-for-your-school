using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationChunk : ContentComponent, IRecommendationChunk<Answer, ContentComponent, Header>
{
    public string Title { get; init; } = null!;

    public Header Header { get; init; } = null!;

    public List<ContentComponent> Content { get; init; } = [];

    public List<Answer> Answers { get; init; } = [];

    public string SlugifiedHeader => Header.Text.Slugify();
}
