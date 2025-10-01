using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class RecommendationsViewModel
{
    public string CategoryName { get; init; } = null!;
    public List<RecommendationChunkEntry> Chunks { get; init; } = [];
    public string? LatestCompletionDate { get; init; } = null!;
    public string SectionName { get; init; } = null!;
    public string SectionSlug { get; init; } = null!;
    public string? Slug { get; init; } = null;
    public IEnumerable<QuestionWithAnswerModel> SubmissionResponses { get; init; } = [];
}
