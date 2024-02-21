using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Models;

public class RecommendationsViewModel
{
    public String SectionName;

    public RecommendationIntro RecommendationIntro;

    public List<RecommendationChunk> RecommendationChunks;
}