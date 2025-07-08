using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Domain.Groups.Models;

public class GroupsCustomRecommendationIntroViewModel
{
    public string HeaderText { get; init; } = null!;

    public string IntroContent { get; init; } = null!;

    public string LinkText { get; init; } = null!;

    public string SelectedEstablishmentName { get; init; } = null!;

    private string? _slugifiedLinkText;

    public string SlugifiedLinkText => _slugifiedLinkText ??= LinkText.Slugify();

    public List<CmsEntryDto> Content => new();

    public List<QuestionWithAnswerModel>? Responses { get; init; }
}
