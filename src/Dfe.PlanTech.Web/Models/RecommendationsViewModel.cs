using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class RecommendationsViewModel
{
    public string SectionName { get; init; } = null!;

    public RecommendationIntro Intro { get; init; } = null!;

    public List<RecommendationChunk> Chunks { get; init; } = null!;

    public IEnumerable<IHeaderWithContent> AllContent => GetAllContent();

    public string Slug { get; init; } = null!;

    private IEnumerable<IHeaderWithContent> GetAllContent()
    {
        yield return Intro;
        foreach (var chunk in Chunks)
        {
            yield return chunk;
        }
    }
}