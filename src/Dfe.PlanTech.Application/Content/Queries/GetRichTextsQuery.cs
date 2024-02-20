using System.Linq;
using System.Linq.Expressions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetRichTextsForPageQuery(ICmsDbContext db, ILogger<GetRichTextsForPageQuery> logger, ContentfulOptions contentfulOptions) : IGetPageChildrenQuery
{
    private readonly ICmsDbContext _db = db;
    private readonly ILogger<GetRichTextsForPageQuery> _logger = logger;
    private readonly ContentfulOptions _contentfulOptions = contentfulOptions;

    /// <summary>
    /// Load RichTextContents for the given page from database 
    /// </summary>
    /// <param name="page"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task TryLoadChildren(PageDbEntity page, CancellationToken cancellationToken)
    {
        try
        {
            var hasTextBodyContents = page.Content.Concat(page.BeforeTitleContent)
                                                .OfType<IHasRichText>()
                                                .Any();

            if (!hasTextBodyContents) return;

            var richTextContentQuery = await _db.RichTextContentWithSlugs.Where(PageMatchesSlugAndPublishedOrIsPreview(page));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching rich text content from Database for {pageId}", page.Id);
            throw;
        }
    }

    private Expression<Func<RichTextContentWithSlugDbEntity, bool>> PageMatchesSlugAndPublishedOrIsPreview(PageDbEntity page)
    {
        return content => content.Slug == page.Slug && (!_contentfulOptions.UsePreview || content.Published);
    }
}