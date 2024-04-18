using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class PageComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["InternalName", "Slug", "DisplayHomeButton", "DisplayBackButton", "DisplayTopicTitle", "DisplayOrganisationName", "RequiresAuthorisation"], "Page")
{
  public override Task ValidateContent()
  {
    ValidatePages(_dbEntities.OfType<PageDbEntity>().ToArray());
    return Task.CompletedTask;
  }

  private void ValidatePages(PageDbEntity[] pages)
  {
    foreach (var contentfulPage in _contentfulEntities)
    {
      ValidatePage(pages, contentfulPage);
    }
  }

  private void ValidatePage(PageDbEntity[] pages, JsonNode contentfulPage)
  {
    var matchingDbPage = pages.FirstOrDefault(header => header.Id == contentfulPage.GetEntryId());

    if (matchingDbPage == null)
    {
      Console.WriteLine($"No matching header found for contentful header with ID: {contentfulPage.GetEntryId()}");
      return;
    }

    var titleIdValidationResult = ValidateChild<PageDbEntity>(matchingDbPage, "TitleId", contentfulPage, "title");
    ValidateProperties(contentfulPage, matchingDbPage, titleIdValidationResult);

    var pageChildren = _db.PageContents.Where(pageContent => pageContent.PageId == matchingDbPage.Id)
                                      .Select(pageContent =>
                                      new
                                      {
                                        contentId = pageContent.ContentComponentId,
                                        beforeTitleContentId = pageContent.BeforeContentComponentId
                                      })
                                      .ToArray();

    matchingDbPage.Content = pageChildren.Where(child => child.contentId != null)
                                        .Select(child => new ContentComponentDbEntity()
                                        {
                                          Id = child.contentId!
                                        }).ToList();

    ValidateChildren(contentfulPage, "content", matchingDbPage, (page) => page.Content);

    matchingDbPage.BeforeTitleContent = pageChildren.Where(child => child.beforeTitleContentId != null)
                                    .Select(child => new ContentComponentDbEntity()
                                    {
                                      Id = child.beforeTitleContentId!
                                    }).ToList();

    ValidateChildren(contentfulPage, "beforeTitleContent", matchingDbPage, (page) => page.BeforeTitleContent);
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.Pages.IgnoreAutoIncludes();
  }
}