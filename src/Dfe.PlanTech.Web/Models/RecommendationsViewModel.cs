using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class RecommendationsViewModel
{
    public String SectionName { get; init; } = null!;

    public RecommendationIntro RecommendationIntro { get; init; } = null!;

    public List<RecommendationChunk> RecommendationChunks { get; init; } = null!;
}