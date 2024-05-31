using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class ShareRecommendation : IHeaderWithContent
{
    public string Title { get; init; } = null!;

    public Header Header { get; init; } = null!;

    public List<ContentComponent> Content { get; init; } = [];

    public string SlugifiedHeader => Header.Text.Slugify();
}
