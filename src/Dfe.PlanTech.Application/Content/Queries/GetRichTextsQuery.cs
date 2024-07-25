using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

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

            var richTextContentQuery = _db.RichTextContentWithSlugs.Where(PageMatchesSlugAndPublishedOrIsPreview(page));

            await _db.ToListAsync(richTextContentQuery, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error fetching RichTextContent from Database for " + page.Id, ex);
        }
    }

    private Expression<Func<RichTextContentWithSlugDbEntity, bool>> PageMatchesSlugAndPublishedOrIsPreview(PageDbEntity page)
    {
        return content => content.Slug == page.Slug && (!_contentfulOptions.UsePreview || content.Published);
    }
}