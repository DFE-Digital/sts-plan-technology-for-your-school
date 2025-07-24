using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Web.Models;

public class RecommendationsViewModel
{
    public string CategoryName { get; init; } = null!;

    public string SectionName { get; init; } = null!;

    public string SectionSlug { get; init; } = null!;

    public List<RecommendationChunk> Chunks { get; init; } = null!;

    public string? LatestCompletionDate { get; init; } = null!;

    public string? Slug { get; init; } = null;

    public IEnumerable<QuestionWithAnswer> SubmissionResponses { get; init; } = null!;
}
