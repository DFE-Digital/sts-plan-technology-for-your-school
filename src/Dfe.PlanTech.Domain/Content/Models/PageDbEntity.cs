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
    public SectionDbEntity? Section { get; set; }

    /// <summary>
    /// Combined joins for <see cref="Content"/> and <see cref="BeforeTitleContent"/>
    /// </summary>
    [DontCopyValue]
    public List<PageContentDbEntity> AllPageContents { get; set; } = [];

    /// <summary>
    /// Gets all content, from both <see cref="Content"/> and <see cref="BeforeTitleContent"/>, that match the type
    /// </summary>
    /// <typeparam name="T">Type of content to find</typeparam>
    public IEnumerable<T> GetAllContentOfType<T>() => Content.Concat(BeforeTitleContent).OfType<T>();

    public void OrderContents()
    {
        BeforeTitleContent = OrderContents(BeforeTitleContent, pageContent => pageContent.BeforeContentComponentId).ToList();
        Content = OrderContents(Content, pageContent => pageContent.ContentComponentId).ToList();
    }

    /// <summary>
    /// Orders the contents based on the value of <see cref="PageContentDbEntity.Order"/>
    /// </summary>
    /// <param name="contents">Contents to order - i.e. <see cref="Content"/> or <see cref="BeforeTitleContent"/> </param>
    /// <param name="idSelector">Func to select the relevant id - i.e.<see cref="PageContentDbEntity.ContentComponentId"/> or <see cref="PageContentDbEntity.BeforeContentComponentId"/></param>
    /// <returns>The contents ordered by the ordering specified in the related <see cref="PageContentDbEntity"/></returns>
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
