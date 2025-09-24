using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser;

[ExcludeFromCodeCoverage]
public class ChunkModel(string answerId, string recommendationHeader)
{
    public string AnswerId { get; set; } = answerId;
    public string RecommendationHeader { get; set; } = recommendationHeader;
}
