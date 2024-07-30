using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationIntro : ContentComponent, IRecommendationIntro<Header, ContentComponent>, IHeaderWithContent
{
    public string Slug { get; init; } = null!;

    public Header Header { get; init; } = null!;

    public string Maturity { get; init; } = null!;

    [NotMapped]
    public List<ContentComponent> Content { get; init; } = [];

    public string SlugifiedHeader => Header.Text.Slugify();
}
