using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.ViewModels;

public class UpdateRecommendationViewModel
{
    public string CategorySlug { get; set; } = string.Empty;
    public string SectionSlug { get; set; } = string.Empty;

    public int RecommendationId { get; set; }
    public string? StatusErrorMessage { get; set; }
    public IDictionary<RecommendationStatus, string> StatusOptions { get; set; } =
        new Dictionary<RecommendationStatus, string>();
    public required RecommendationStatus SelectedStatusKey { get; init; }
    public RecommendationChunkEntry CurrentChunk { get; set; } = null!;


}
