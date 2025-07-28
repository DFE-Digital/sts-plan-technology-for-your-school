using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Web.Models;

public class RecommendationsViewModel
{
    public IEnumerable<IHeaderWithContent> AllContent => GetAllContent();
    public string SectionName { get; init; } = null!;
    public string SectionSlug { get; init; } = null!;
    public CmsRecommendationIntroDto Intro { get; init; } = null!;
    public List<CmsRecommendationChunkDto> Chunks { get; init; } = null!;
    public string? LatestCompletionDate { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public IEnumerable<QuestionWithAnswerModel> SubmissionResponses { get; init; } = null!;

    private IEnumerable<IHeaderWithContent> GetAllContent()
    {
        yield return Intro;
        foreach (var chunk in Chunks)
        {
            yield return chunk;
        }
    }
}
