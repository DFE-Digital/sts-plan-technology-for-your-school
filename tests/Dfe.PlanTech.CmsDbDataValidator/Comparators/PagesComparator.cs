using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class PageComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["InternalName", "Slug", "DisplayHomeButton", "DisplayBackButton", "DisplayTopicTitle", "DisplayOrganisationName", "RequiresAuthorisation", "TitleId"], "Page")
{
  /*
      Assert.Equal(contentfulEntry["title"]?["sys"]?["id"]?.GetValue<string>(), dbEntry.TitleId);

      _db.Entry(dbEntry).Collection(page => page.Content).Load();
      comparator.ValidateArrayReferences(contentfulEntry, "content", dbEntry, (entry) => entry.Content);

      _db.Entry(dbEntry).Collection(page => page.BeforeTitleContent).Load();
      comparator.ValidateArrayReferences(contentfulEntry, "beforeTitleContent", dbEntry, (entry) => entry.BeforeTitleContent);
*/
  public override async Task ValidateContent()
  {
    await ValidatePages(_dbEntities.OfType<PageDbEntity>().ToArray());
  }

  private async Task ValidatePages(PageDbEntity[] pages)
  {
    foreach (var contentfulPage in _contentfulEntities)
    {
      await ValidatePage(pages, contentfulPage);
    }
  }

  private async Task ValidatePage(PageDbEntity[] pages, JsonNode contentfulPage)
  {
    var matchingDbPage = pages.FirstOrDefault(header => header.Id == contentfulPage.GetEntryId());

    if (matchingDbPage == null)
    {
      Console.WriteLine($"No matching header found for contentful header with ID: {contentfulPage.GetEntryId()}");
      return;
    }

    var titleIdValidationResult = ValidateChild<PageDbEntity>(matchingDbPage, "TitleId", contentfulPage, "title");
    ValidateProperties(contentfulPage, matchingDbPage, titleIdValidationResult);

    await _db.Entry(matchingDbPage).Collection(page => page.Content).LoadAsync();
    ValidateChildren(contentfulPage, "content", matchingDbPage, (page) => page.Content);

    await _db.Entry(matchingDbPage).Collection(page => page.BeforeTitleContent).LoadAsync();
    ValidateChildren(contentfulPage, "beforeTitleContent", matchingDbPage, (page) => page.BeforeTitleContent);

  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.Headers;
  }
}