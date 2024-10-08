using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetButtonWithEntryReferencesQuery(ICmsDbContext db, ILogger<GetButtonWithEntryReferencesQuery> logger)
    : IGetPageChildrenQuery
{
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
            var pageButtonWithEntryReferences = page.GetAllContentOfType<ButtonWithEntryReferenceDbEntity>().ToArray();

            if (pageButtonWithEntryReferences.Length == 0)
                return;

            var buttonQuery = ButtonWithEntryReferencesQueryable(page);

            var buttons = await db.ToListAsync(buttonQuery, cancellationToken);

            foreach (var button in buttons)
            {
                var matching = pageButtonWithEntryReferences.FirstOrDefault(pb => pb.Id == button.Id);

                if (matching == null)
                {
                    logger.LogError("Couldn't find matching button for Id {Id}", button.Id);
                    continue;
                }

                matching.LinkToEntry = button.LinkToEntry;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading button references for {page}", page.Id);
            throw new InvalidOperationException("An unexpected error occurred while loading button references.", ex);
        }
    }

    /// <summary>
    /// Quer to get <see cref="ButtonWithEntryReferenceDbEntity">s for the given page, but with only necessary information we require
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    private IQueryable<ButtonWithEntryReferenceDbEntity> ButtonWithEntryReferencesQueryable(PageDbEntity page)
    {
        var buttonWithEntryReferences = db.ButtonWithEntryReferences.Where(button => button.ContentPages.Any(contentPage => contentPage.Id == page.Id));

        var pageButtons = db.Pages.Where(p => buttonWithEntryReferences.Any(button => button.LinkToEntryId == p.Id))
                                    .Select(p => new
                                    {
                                        p.Id,
                                        p.Slug
                                    });

        var questionButtons = db.Questions.Where(question => buttonWithEntryReferences.Any(button => button.LinkToEntryId == page.Id))
                                            .Select(q => new
                                            {
                                                q.Id,
                                                q.Slug
                                            });

        return buttonWithEntryReferences.Select(button => new
        {
            button.Id,
            page = pageButtons.FirstOrDefault(pageButton => pageButton.Id == button.Id),
            question = questionButtons.FirstOrDefault(quesionButton => quesionButton.Id == button.Id),
        }).Select(button => new ButtonWithEntryReferenceDbEntity
        {
            Id = button.Id,
            LinkToEntry = button.page != null ?
                            new PageDbEntity() { Slug = button.page.Slug } :
                            new QuestionDbEntity() { Slug = button.question!.Slug }
        });
    }
}
