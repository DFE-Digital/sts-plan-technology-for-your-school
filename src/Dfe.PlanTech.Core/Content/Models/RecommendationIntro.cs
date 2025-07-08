using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationIntro : ContentComponent, IRecommendationIntro<ComponentHeaderEntry, ContentComponent>, IHeaderWithContent
{
    public string Slug { get; init; } = null!;

    public ComponentHeaderEntry Header { get; init; } = null!;

    public string Maturity { get; init; } = null!;

    [NotMapped]
    public List<ContentComponent> Content { get; init; } = [];

    public string HeaderText => Header.Text;

    public string LinkText => "Overview";

    private string? _slugifiedLinkText;

    public string SlugifiedLinkText => _slugifiedLinkText ??= LinkText.Slugify();
}
