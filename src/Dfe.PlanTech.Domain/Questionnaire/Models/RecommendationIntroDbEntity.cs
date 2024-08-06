using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationIntroDbEntity : ContentComponentDbEntity, IRecommendationIntro<HeaderDbEntity, ContentComponentDbEntity>
{
    public string Slug { get; init; } = null!;

    public string HeaderId { get; set; } = null!;

    public HeaderDbEntity Header { get; init; } = null!;

    public string Maturity { get; init; } = null!;


    [DontCopyValue]
    public List<ContentComponentDbEntity> Content { get; init; } = [];

    [DontCopyValue]
    public List<SubtopicRecommendationDbEntity> SubtopicRecommendations { get; set; } = [];
}
