using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

/// <summary>
/// Class for the <see cref="Section"/> table in the database 
/// </summary>
public class SectionDbEntity : ContentComponentDbEntity, ISection<QuestionDbEntity, PageDbEntity, RecommendationPageDbEntity>
{
    public string Name { get; set; } = null!;

    public List<QuestionDbEntity> Questions { get; set; } = new();

    public PageDbEntity? InterstitialPage { get; set; } = null!;

    public string? InterstitialPageId { get; set; } = null!;

    public CategoryDbEntity? Category { get; set; }

    public string? CategoryId { get; set; }

    public List<RecommendationPageDbEntity> Recommendations { get; set; } = new();
}
