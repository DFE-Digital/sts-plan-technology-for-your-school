using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Models;

namespace Dfe.PlanTech.Web.Models;

public class RecommendationsViewModel
{
    public string SectionName { get; init; } = null!;

    public string SectionSlug { get; init; } = null!;

    public RecommendationIntro Intro { get; init; } = null!;

    public List<RecommendationChunk> Chunks { get; init; } = null!;

    public string? LatestCompletionDate { get; init; } = null!;

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
    public IEnumerable<Domain.Submissions.Models.QuestionWithAnswerModel> SubmissionResponses { get; init; } = null!;
}
