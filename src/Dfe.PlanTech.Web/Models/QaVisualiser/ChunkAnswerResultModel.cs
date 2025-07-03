namespace Dfe.PlanTech.Web.Models.QaVisualiser
{
    public class ChunkAnswerResultModel(string answerId, string recommendationHeader)
    {
        public string AnswerId { get; set; } = answerId;
        public string RecommendationHeader { get; set; } = recommendationHeader;
    }
}
