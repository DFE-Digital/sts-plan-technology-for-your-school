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

    public List<ContentComponentDbEntity> BeforeTitleContent { get; set; } = [];

    public TitleDbEntity? Title { get; set; }

    public string? TitleId { get; set; }

    public List<ContentComponentDbEntity> Content { get; set; } = [];

    public RecommendationPageDbEntity? RecommendationPage { get; set; }

    public SectionDbEntity? Section { get; set; }

    /// <summary>
    /// Combined joins for <see cref="Content"/> and <see cref="BeforeTitleContent"/> 
    /// </summary>
    public List<PageContentDbEntity> PageContents { get; set; } = [];

    [NotMapped]
    public IEnumerable<ContentComponentDbEntity> OrderedBeforeTitleContent => PageContents.Where(content => content.BeforeContentComponent != null && content.ContentComponent == null)
                                                                                    .OrderBy(content => content.Order)
                                                                                    .Select(content => content.BeforeContentComponent!);

    [NotMapped]
    public IEnumerable<ContentComponentDbEntity> OrderedContent => PageContents.Where(content => content.BeforeContentComponent == null && content.ContentComponent != null)
                                                                                    .OrderBy(content => content.Order)
                                                                                    .Select(content => content.ContentComponent!);

    public void OrderContents()
    {
        BeforeTitleContent = OrderContents(BeforeTitleContent, pageContent => pageContent.BeforeContentComponentId).ToList();
        Content = OrderContents(Content, pageContent => pageContent.ContentComponentId).ToList();
    }

    private IEnumerable<ContentComponentDbEntity> OrderContents(List<ContentComponentDbEntity> contents, Func<PageContentDbEntity, string?> idSelector)
        => contents.Join(PageContents,
                            content => content.Id,
                            idSelector,
                            (content, pageContent) => new
                            {
                                content,
                                order = pageContent.Order
                            })
                    .OrderBy(content => content.order)
                    .Select(content => content.content);
}