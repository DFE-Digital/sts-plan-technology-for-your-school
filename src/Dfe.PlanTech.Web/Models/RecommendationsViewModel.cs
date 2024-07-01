using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class RecommendationsViewModel
{
    public string SectionName { get; init; } = null!;

    public RecommendationIntro Intro { get; init; } = null!;

    public List<RecommendationChunk> Chunks { get; init; } = null!;

    public IEnumerable<IHeaderWithContent> AllContent => GetAllContent();

    public IEnumerable<IAccordion> Accordions => Chunks.Select(ConvertToAccordion);

    public string Slug { get; init; } = null!;

    private static Accordion ConvertToAccordion(IHeaderWithContent content, int index)
    => new()
    {
        Header = content.Header.Text,
        Order = index + 1,
        Slug = content.SlugifiedHeader,
        Title = content.Title,
    };

    private IEnumerable<IHeaderWithContent> GetAllContent()
    {
        yield return Intro;
        foreach (var chunk in Chunks)
        {
            yield return chunk;
        }
    }
}