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

    public string? SectionTitle { get; set; }

    public ContentComponentDbEntity[] BeforeTitleContent { get; set; } = Array.Empty<ContentComponentDbEntity>();

    public TitleDbEntity? Title { get; set; }

    public string? TitleId { get; set; }

    public string? OrganisationName { get; set; }

    public ContentComponentDbEntity[] Content { get; set; } = Array.Empty<ContentComponentDbEntity>();
}
