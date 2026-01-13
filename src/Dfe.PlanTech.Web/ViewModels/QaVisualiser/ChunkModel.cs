using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser;

[ExcludeFromCodeCoverage]
public class ChunkModel(string completingAnswerId, string inProgressAnswerId, string recommendationHeader)
{
    public string? CompletingAnswerId { get; set; } = completingAnswerId;
    public string? InProgressAnswerId { get; set; } = inProgressAnswerId;
    public string RecommendationHeader { get; set; } = recommendationHeader;
}
