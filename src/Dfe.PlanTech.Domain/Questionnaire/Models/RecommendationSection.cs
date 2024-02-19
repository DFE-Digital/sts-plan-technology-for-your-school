using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationSection : ContentComponent, IRecommendationSection<Answer, RecommendationChunk>
{
    public List<Answer> Answers { get; init; } = [];

    public List<RecommendationChunk> RecommendationChunks { get; init; } = [];
}