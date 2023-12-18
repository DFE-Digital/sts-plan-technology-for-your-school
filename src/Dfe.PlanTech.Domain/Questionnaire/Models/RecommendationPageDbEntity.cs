using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class RecommendationPageDbEntity : ContentComponentDbEntity, IRecommendationPage<PageDbEntity>
{
    public string InternalName { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public Maturity Maturity { get; set; }

    public PageDbEntity Page { get; set; } = null!;

    public string PageId { get; set; } = null!;

    public Section? Section { get; set; }

    public string? SectionId { get; set; }
}
