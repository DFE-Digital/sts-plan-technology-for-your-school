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
    public List<ContentComponentDbEntity> BeforeTitleContent { get; set; } = [];

    [DontCopyValue]
    public TitleDbEntity? Title { get; set; }

    public string? TitleId { get; set; }

    [DontCopyValue]
    public List<ContentComponentDbEntity> Content { get; set; } = [];
    [DontCopyValue]

    [DontCopyValue]
    public SectionDbEntity? Section { get; set; }

    [DontCopyValue] public List<PageContentDbEntity> AllPageContents { get; set; } = [];

    public IEnumerable<T> GetAllContentOfType<T>() => Content.Concat(BeforeTitleContent).OfType<T>();

    public void OrderContents()
    {
        BeforeTitleContent = OrderContents(BeforeTitleContent, pageContent => pageContent.BeforeContentComponentId).ToList();
        Content = OrderContents(Content, pageContent => pageContent.ContentComponentId).ToList();
    }

    private IEnumerable<ContentComponentDbEntity> OrderContents(List<ContentComponentDbEntity> contents, Func<PageContentDbEntity, string?> idSelector)
        => contents.GroupJoin(AllPageContents,
                content => content.Id,
                idSelector,
                (content, pageContent) => new
                {
                    content,
                    order = pageContent.OrderByDescending(pc => pc.Id).Select(join => join.Order).First()
                })
            .OrderBy(joined => joined.order)
            .Select(joined => joined.content);
}
