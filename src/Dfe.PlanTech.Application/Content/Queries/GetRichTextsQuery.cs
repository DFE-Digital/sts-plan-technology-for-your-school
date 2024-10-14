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
            var richTextParents = page.GetAllContentOfType<IHasRichText>().ToArray();

            if (richTextParents.Length == 0)
                return;

            var richTextContentQuery = _db.RichTextContentWithSlugs.Where(PageMatchesSlugAndPublishedOrIsPreview(page));

            var richTexts = await _db.ToListCachedAsync(richTextContentQuery, cancellationToken);

            foreach (var parent in richTextParents)
            {
                var matching = richTexts.FirstOrDefault(rt => rt.Id == parent.RichTextId);

                if (matching == null)
                {
                    logger.LogError("Unable to find matching rich text for rich text Id {Id}", parent.RichTextId);
                    continue;
                }

                parent.RichText = matching;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching RichTextContent from Database for {PageId}", page.Id);
            throw new InvalidOperationException("Error fetching RichTextContent from Database for " + page.Id, ex);
        }
    }

    private Expression<Func<RichTextContentWithSlugDbEntity, bool>> PageMatchesSlugAndPublishedOrIsPreview(PageDbEntity page)
    {
        return content => content.Slug == page.Slug && (_contentfulOptions.UsePreview || content.Published);
    }
}
