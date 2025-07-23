using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models
{
    public class SingleRecommendationViewModel
    {
        public string CategorySlug { get; set; } = "";

        public string CategoryName { get; set; } = "";

        public Section Section { get; set; } = null!;

        public List<RecommendationChunk> Chunks { get; set; } = [];

        public RecommendationChunk CurrentChunk { get; set; } = null!;

        public RecommendationChunk? PreviousChunk { get; set; } = null!;

        public RecommendationChunk? NextChunk { get; set; } = null!;

        public int CurrentChunkPosition { get; set; }

        public int TotalChunks { get; set; }
    }
}
