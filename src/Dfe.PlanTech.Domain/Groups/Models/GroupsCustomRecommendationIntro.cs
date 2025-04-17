using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Groups.Models;

public class GroupsCustomRecommendationIntro : IHeaderWithContent
{
    public string HeaderText { get; init; } = null!;

    public string IntroContent { get; init; } = null!;

    public string LinkText { get; init; } = null!;

    public string SelectedEstablishmentName { get; init; } = null!;

    private string? _slugifiedLinkText;

    public string SlugifiedLinkText => _slugifiedLinkText ??= LinkText.Slugify();

    public List<ContentComponent> Content => new();

    public List<QuestionWithAnswer>? Responses { get; init; }
}
