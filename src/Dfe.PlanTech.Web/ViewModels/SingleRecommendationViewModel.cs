using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

public class SingleRecommendationViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string SectionSlug { get; set; } = string.Empty;
    public QuestionnaireSectionEntry Section { get; set; } = null!;
    public List<RecommendationChunkEntry> Chunks { get; set; } = [];
    public RecommendationChunkEntry CurrentChunk { get; set; } = null!;
    public RecommendationChunkEntry? PreviousChunk { get; set; } = null!;
    public RecommendationChunkEntry? NextChunk { get; set; } = null!;
    public int CurrentChunkPosition { get; set; }
    public int TotalChunks { get; set; }
}
