using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class RecommendationsViewModel
{
    public string SectionName { get; init; } = null!;

    public RecommendationIntro RecommendationIntro { get; init; } = null!;

    public List<RecommendationChunk> RecommendationChunks { get; init; } = null!;

    public IEnumerable<IHeaderWithContent> AllContent => GetAllContent();

    private IEnumerable<IHeaderWithContent> GetAllContent()
    {
        yield return RecommendationIntro;
        foreach (var chunk in RecommendationChunks)
        {
            yield return chunk;
        }
    }
}