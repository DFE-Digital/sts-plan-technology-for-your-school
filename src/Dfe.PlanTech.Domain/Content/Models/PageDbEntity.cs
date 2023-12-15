using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Models;

public class PageDbEntity : ContentComponentDbEntity, IPage<ContentComponentDbEntity, TitleDbEntity>
{
    public string InternalName { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public bool DisplayBackButton { get; set; }

    public bool DisplayHomeButton { get; set; }

    public bool DisplayTopicTitle { get; set; }

    public bool DisplayOrganisationName { get; set; }

    public bool RequiresAuthorisation { get; set; } = true;

    public List<ContentComponentDbEntity> BeforeTitleContent { get; set; } = new();

    public TitleDbEntity? Title { get; set; }

    public string? TitleId { get; set; }

    public List<ContentComponentDbEntity> Content { get; set; } = new();

    public RecommendationPageDbEntity? RecommendationPage { get; set; }

    public SectionDbEntity? Section { get; set; }

    public string? SectionId { get; set; }
}
