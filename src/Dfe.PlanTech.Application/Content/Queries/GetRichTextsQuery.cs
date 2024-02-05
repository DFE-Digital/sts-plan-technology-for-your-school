using Dfe.PlanTech.Application.Persistence.Interfaces;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetRichTextsQuery : IGetPageChildrenQuery
{
    private readonly ICmsDbContext _db;
    private readonly ILogger<GetRichTextsQuery> _logger;

    public GetRichTextsQuery(ICmsDbContext db, ILogger<GetRichTextsQuery> logger)
    {
        _db = db;
        _logger = logger;
    }

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

            await _db.ToListAsync(_db.RichTextContentsByPageSlug(page.Slug), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching rich text content from Database for {pageId}", page.Id);
            throw;
        }
    }
}