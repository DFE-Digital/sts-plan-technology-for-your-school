using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationChunk : ContentComponent, IRecommendationChunk<QuestionnaireAnswerEntry, ContentComponent>, IHeaderWithContent
{
    public string Header { get; init; } = null!;

    public List<ContentComponent> Content { get; init; } = [];

    public List<QuestionnaireAnswerEntry> Answers { get; init; } = [];

    public CSLink? CSLink { get; init; }

    public string HeaderText => Header;

    public string LinkText => HeaderText;

    private string? _slugifiedLinkText;

    public string SlugifiedLinkText => _slugifiedLinkText ??= LinkText.Slugify();
}
