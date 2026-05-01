using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class SingleRecommendationModel
{
    public RecommendationStatus? SelectedStatus { get; set; } = null;
    public string? Notes { get; set; } = null;
}
