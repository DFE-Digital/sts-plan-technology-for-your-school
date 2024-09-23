using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class PageEntityUpdater : EntityUpdater
{

    public PageEntityUpdater(ILogger<PageEntityUpdater> logger,
                                   ICmsDbContext db,
                                   IDatabaseHelper<ICmsDbContext> databaseHelper) : base(logger, db, databaseHelper)
    {
    }

    public override MappedEntity UpdateEntityConcrete(MappedEntity entity)
    {
        if (!entity.AlreadyExistsInDatabase)
        {
            return entity;
        }

        if (entity.IncomingEntity is not PageDbEntity incomingPage || entity.ExistingEntity is not PageDbEntity existingPage)
        {
            throw new InvalidCastException($"Entities are not expected page types. Received {entity.IncomingEntity.GetType()} and {entity.ExistingEntity!.GetType()}");
        }

        AddOrUpdatePageContents(incomingPage, existingPage);
        DeleteRemovedPageContents(incomingPage, existingPage);

        return entity;
    }

    /// <summary>
    /// Removes page contents from existing page if they are not in the incoming page update
    /// </summary>
    /// <param name="incomingPage"></param>
    /// <param name="existingPage"></param>
    private void DeleteRemovedPageContents(PageDbEntity incomingPage, PageDbEntity existingPage)
    {
        var contentsToRemove = existingPage.AllPageContents.Where(pageContent => !HasPageContent(incomingPage, pageContent))
                                                            .ToArray();

        foreach (var pageContent in contentsToRemove)
        {
            existingPage.AllPageContents.Remove(pageContent);
            DatabaseHelper.Remove(pageContent);
        }
    }

    private static bool HasPageContent(PageDbEntity page, PageContentDbEntity pageContent)
     => page.AllPageContents.Exists(pc => pc.Matches(pageContent));

    /// <summary>
    /// For each incoming page content:
    /// - Add if it doesn't already exist
    /// - Remove duplicates if any
    /// - Update order if existing and order has changed
    /// </summary>
    /// <param name="incomingPage"></param>
    /// <param name="existingPage"></param>
    private void AddOrUpdatePageContents(PageDbEntity incomingPage, PageDbEntity existingPage)
    {
        foreach (var pageContent in incomingPage.AllPageContents)
        {
            var matchingContents = existingPage.AllPageContents.Where(pc => pc.Matches(pageContent))
                                                              .OrderByDescending(pc => pc.Id)
                                                              .ToList();

            ProcessPageContent(existingPage, pageContent, matchingContents);
        }
    }

    /// <summary>
    /// - Add page content if missing from existing page
    /// - Remove duplicates from existing page if found
    /// - Update order if changed
    /// </summary>
    /// <param name="existingPage"></param>
    /// <param name="pageContent"></param>
    /// <param name="matchingContents"></param>
    private void ProcessPageContent(PageDbEntity existingPage, PageContentDbEntity pageContent, List<PageContentDbEntity> matchingContents)
    {
        if (matchingContents.Count == 0)
        {
            existingPage.AllPageContents.Add(pageContent);
            return;
        }

        RemoveDuplicates(existingPage, matchingContents);
        UpdatePageContentOrder(pageContent, matchingContents);
    }

    /// <summary>
    /// Updates the order of the page content if it's different
    /// </summary>
    /// <param name="pageContent"></param>
    /// <param name="matchingContents"></param>
    private static void UpdatePageContentOrder(PageContentDbEntity pageContent, List<PageContentDbEntity> matchingContents)
    {
        var remainingMatchingContent = matchingContents[0];

        //Only change the order if it has actually changed, to prevent unnecessary updates to the DB by EF Core
        if (remainingMatchingContent.Order != pageContent.Order)
        {
            remainingMatchingContent.Order = pageContent.Order;
        }
    }

    /// <summary>
    /// Remove duplicate page content entities
    /// </summary>
    /// <remarks>
    /// Shouldn't be needed anymore, but exists to fix old data issues if they arise.
    /// </remarks>
    /// <param name="existingPage"></param>
    /// <param name="matchingContents"></param>
    private void RemoveDuplicates(PageDbEntity existingPage, List<PageContentDbEntity> matchingContents)
    {
        if (matchingContents.Count > 1)
        {
            foreach (var contentToRemove in matchingContents.Skip(1))
            {
                existingPage.AllPageContents.Remove(contentToRemove);
                DatabaseHelper.Remove(contentToRemove);
            }
        }
    }
}
