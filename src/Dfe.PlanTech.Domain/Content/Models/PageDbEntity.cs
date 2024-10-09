using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Content.Models;

public class PageDbEntity : ContentComponentDbEntity, IPage<ContentComponentDbEntity, TitleDbEntity>, IHasSlug
{
    public string InternalName { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public bool DisplayBackButton { get; set; }

    public bool DisplayHomeButton { get; set; }

    public bool DisplayTopicTitle { get; set; }

    public bool DisplayOrganisationName { get; set; }

    public bool RequiresAuthorisation { get; set; } = true;

    [DontCopyValue]
    [NotMapped]
    public List<ContentComponentDbEntity> BeforeTitleContent { get; set; } = [];

    [DontCopyValue]
    public TitleDbEntity? Title { get; set; }

    public string? TitleId { get; set; }

    [DontCopyValue]
    [NotMapped]
    public List<ContentComponentDbEntity> Content { get; set; } = [];

    [DontCopyValue]
    public SectionDbEntity? Section { get; set; }

    public void OrderContents()
    {
    }

    public IEnumerable<T> GetAllContentOfType<T>() => Content.Concat(BeforeTitleContent).OfType<T>();
}
