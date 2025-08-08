using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.CategoryLanding;

public class CategoryLandingSectionRecommendations
{
    public string SectionName { get; init; } = null!;

    public string SectionSlug { get; init; } = null!;

    public List<QuestionWithAnswer> Answers { get; init; } = null!;

    public List<RecommendationChunk> Chunks { get; init; } = null!;

    public string? NoRecommendationFoundErrorMessage { get; init; } = null!;
}
