namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser
{
    public class ChunkModel(string answerId, string recommendationHeader)
    {
        public string AnswerId { get; set; } = answerId;
        public string RecommendationHeader { get; set; } = recommendationHeader;
    }
}
