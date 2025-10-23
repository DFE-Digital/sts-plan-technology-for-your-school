using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.ViewModels;

public class RecommendationChunkViewModel
{
    public string HeaderText { get; init; } = null!;
    public DateTime LastUpdated { get; set; }
    public RecommendationStatus Status { get; set; }
    public string SlugifiedLinkText { get; set; } = null!;
}
