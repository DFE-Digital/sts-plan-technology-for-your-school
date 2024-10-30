using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationChunk : ContentComponent, IRecommendationChunk<Answer, ContentComponent>, IHeaderWithContent
{
    private string? _slugifiedHeader;

    public string Header { get; init; } = null!;

    public List<ContentComponent> Content { get; init; } = [];

    public List<Answer> Answers { get; init; } = [];

    public string SlugifiedHeader => _slugifiedHeader ??= Header.Slugify();

    public CSLink? CSLink { get; init; }

    public string HeaderText => Header;
}
