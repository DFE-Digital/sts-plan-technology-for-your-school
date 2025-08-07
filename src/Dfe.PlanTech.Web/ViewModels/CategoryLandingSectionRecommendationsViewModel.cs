using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Web.ViewModels;

public class CategoryLandingSectionRecommendationsViewModel
{
    public string? NoRecommendationFoundErrorMessage { get; init; }
    public List<QuestionWithAnswerModel> Answers { get; init; } = null!;
    public List<CmsRecommendationChunkDto> Chunks { get; init; } = null!;
    public string? SectionName { get; init; }
    public string? SectionSlug { get; init; }
    public bool? Viewed { get; init; }
}
