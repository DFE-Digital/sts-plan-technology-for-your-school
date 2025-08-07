using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Web.ViewModels;

public class GroupsRecommendationsViewModel
{
    public string SelectedEstablishmentName { get; set; } = null!;

    public int SelectedEstablishmentId { get; set; }

    public string SectionName { get; init; } = null!;

    public CmsRecommendationIntroDto? Intro { get; init; } = null!;

    public List<CmsRecommendationChunkDto> Chunks { get; init; } = null!;

    public string Slug { get; init; } = null!;

    public IHeaderWithContent? GroupsCustomRecommendationIntro { get; init; }

    public IEnumerable<IHeaderWithContent> AllContent => GetAllContent();

    private IEnumerable<IHeaderWithContent> GetAllContent()
    {
        if (Intro != null)
        {
            yield return Intro;
        }

        if (GroupsCustomRecommendationIntro != null)
        {
            yield return GroupsCustomRecommendationIntro;
        }

        foreach (var chunk in Chunks)
        {
            yield return chunk;
        }
    }

    public IEnumerable<QuestionWithAnswerModel> SubmissionResponses { get; init; } = null!;
}
