using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class RecommendationsViewModel
{
    public required string CategoryName { get; init; }
    public List<RecommendationChunkViewModel> Chunks { get; init; } = [];
    public string? LatestCompletionDate { get; init; }
    public required string SectionName { get; init; }
    public required string SectionSlug { get; init; }
    public string? Slug { get; init; }
    public int? CurrentChunkCount { get; set; }
    public int? TotalChunks { get; set; }
    public IEnumerable<QuestionWithAnswerModel> SubmissionResponses { get; init; } = [];
}
