using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetButtonWithEntryReferencesQuery : IGetPageChildrenQuery
{
    private readonly ICmsDbContext _db;
    private readonly ILogger<GetButtonWithEntryReferencesQuery> _logger;

    public GetButtonWithEntryReferencesQuery(ICmsDbContext db, ILogger<GetButtonWithEntryReferencesQuery> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// If the Page.Content has any <see cref="ButtonWithEntryReferenceDbEntity"/>s, load the link to entry reference from the database for it
    /// </summary>
    /// <param name="page"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task TryLoadChildren(PageDbEntity page, CancellationToken cancellationToken)
    {
        try
        {
            var buttons = page.Content.Exists(content => content is ButtonWithEntryReferenceDbEntity);

            if (!buttons)
                return;

            var buttonQuery = ButtonWithEntryReferencesQueryable(page);

            await _db.ToListAsync(buttonQuery, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading button references for {page}", page.Id);
            throw new InvalidOperationException("An unexpected error occurred while loading button references.", ex);
        }
    }

    /// <summary>
    /// Quer to get <see cref="ButtonWithEntryReferenceDbEntity">s for the given page, but with only necessary information we require
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    private IQueryable<ButtonWithEntryReferenceDbEntity> ButtonWithEntryReferencesQueryable(PageDbEntity page)
    => _db.ButtonWithEntryReferences.Where(button => button.ContentPages.Any(contentPage => contentPage.Id == page.Id))
                                    .Select(button => new ButtonWithEntryReferenceDbEntity()
                                    {
                                        Id = button.Id,
                                        LinkToEntry = new PageDbEntity()
                                        {
                                            Slug = ((IHasSlug)button.LinkToEntry).Slug
                                        }
                                    });
}
