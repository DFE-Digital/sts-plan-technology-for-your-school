using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Web.Models;

public class RecommendationsViewModel
{
    public string CategoryName { get; init; } = null!;

    public string SectionName { get; init; } = null!;

    public string SectionSlug { get; init; } = null!;

//    public RecommendationIntro Intro { get; init; } = null!;

    public List<RecommendationChunk> Chunks { get; init; } = null!;

    public string? LatestCompletionDate { get; init; } = null!;

//    public IEnumerable<IHeaderWithContent> AllContent => GetAllContent();

    public string? Slug { get; init; } = null;

    //private IEnumerable<IHeaderWithContent> GetAllContent()
    //{
    //    foreach (var chunk in Chunks)
    //    {
    //        yield return chunk;
    //    }
    //}
    public IEnumerable<QuestionWithAnswer> SubmissionResponses { get; init; } = null!;
}
