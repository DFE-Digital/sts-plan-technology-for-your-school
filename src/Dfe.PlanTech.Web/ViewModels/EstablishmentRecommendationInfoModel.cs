using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class EstablishmentRecommendationInfoModel
{
    public int EstablishmentId { get; set; }
    public string EstablishmentName { get; set; } = null!;
    public string EstablishmentRef { get; set; } = null!;
    public string SectionId { get; set; } = null!;
    public int? EstablishmentRecommendationHistoryId { get; set; }
    public string? DateLastUpdated { get; set; }
    public RecommendationStatus Status { get; set; }
}
