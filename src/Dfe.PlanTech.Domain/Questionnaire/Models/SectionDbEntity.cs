using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

/// <summary>
/// Class for the <see cref="Section"/> table in the database
/// </summary>
public class SectionDbEntity : ContentComponentDbEntity, ISection<QuestionDbEntity, PageDbEntity>
{
    public string InternalName { get; set; } = null!;

    public string Name { get; set; } = null!;

    [DontCopyValue]
    public List<QuestionDbEntity> Questions { get; set; } = [];

    [DontCopyValue]
    public PageDbEntity? InterstitialPage { get; set; }

    public string? InterstitialPageId { get; set; }

    [DontCopyValue]
    public CategoryDbEntity? Category { get; set; }

    [DontCopyValue]
    public string? CategoryId { get; set; }

    [DontCopyValue]
    public SubtopicRecommendationDbEntity? SubtopicRecommendation { get; set; }
}
