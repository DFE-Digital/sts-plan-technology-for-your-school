using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.ViewModels;

public class SingleRecommendationViewModel
{
    public string CategoryName { get; set; } = "";

    public string CategorySlug { get; set; } = "";

    public CmsQuestionnaireSectionDto Section { get; set; } = null!;

    public List<CmsRecommendationChunkDto> Chunks { get; set; } = [];

    public CmsRecommendationChunkDto CurrentChunk { get; set; } = null!;

    public CmsRecommendationChunkDto? PreviousChunk { get; set; } = null!;

    public CmsRecommendationChunkDto? NextChunk { get; set; } = null!;

    public int CurrentChunkPosition { get; set; }

    public int TotalChunks { get; set; }
}
