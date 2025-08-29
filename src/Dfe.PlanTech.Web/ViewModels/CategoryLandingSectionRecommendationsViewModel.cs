using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

public class CategoryLandingSectionRecommendationsViewModel
{
    public string? NoRecommendationFoundErrorMessage { get; init; }
    public List<QuestionWithAnswerModel> Answers { get; init; } = null!;
    public List<RecommendationChunkEntry> Chunks { get; init; } = null!;
    public string? SectionName { get; init; }
    public string? SectionSlug { get; init; }
    public bool? Viewed { get; init; }
}
