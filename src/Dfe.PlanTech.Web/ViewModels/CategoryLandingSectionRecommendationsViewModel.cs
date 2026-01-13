using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class CategoryLandingSectionRecommendationsViewModel
{
    public string? NoRecommendationFoundErrorMessage { get; init; }
    public List<QuestionWithAnswerModel> Answers { get; init; } = [];
    public List<RecommendationChunkViewModel> Chunks { get; init; } = [];
    public string? SectionName { get; init; }
    public string? SectionSlug { get; init; }
    public bool? Viewed { get; init; }
}
