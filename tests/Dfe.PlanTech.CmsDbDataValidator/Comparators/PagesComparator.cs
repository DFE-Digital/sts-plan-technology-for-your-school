using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Models;
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
    var matchingDbPage = TryRetrieveMatchingDbEntity(pages, contentfulPage);
    if (matchingDbPage == null)
    {
      return;
    }

    ValidateProperties(contentfulPage, matchingDbPage, GetDataValidationErrors(matchingDbPage, contentfulPage).ToArray());
  }

  protected IEnumerable<DataValidationError> GetDataValidationErrors(PageDbEntity dbPage, JsonNode contentfulPage)
  {
    var titleIdValidationResult = TryGenerateDataValidationError("Title", ValidateChild<PageDbEntity>(dbPage, "TitleId", contentfulPage, "title"));
    if (titleIdValidationResult != null)
    {
      yield return titleIdValidationResult;
    }

    var pageChildren = _db.PageContents.Where(pageContent => pageContent.PageId == dbPage.Id)
                                      .Select(pageContent =>
                                      new
                                      {
                                        contentId = pageContent.ContentComponentId,
                                        beforeTitleContentId = pageContent.BeforeContentComponentId
                                      })
                                      .ToArray();

    dbPage.Content = pageChildren.Where(child => child.contentId != null)
                                        .Select(child => new ContentComponentDbEntity()
                                        {
                                          Id = child.contentId!
                                        }).ToList();

    foreach (var child in ValidateChildren(contentfulPage, "content", dbPage, (page) => page.Content))
    {
      yield return child;
    }

    dbPage.BeforeTitleContent = pageChildren.Where(child => child.beforeTitleContentId != null)
                                    .Select(child => new ContentComponentDbEntity()
                                    {
                                      Id = child.beforeTitleContentId!
                                    }).ToList();

    foreach (var child in ValidateChildren(contentfulPage, "beforeTitleContent", dbPage, (page) => page.BeforeTitleContent))
    {
      yield return child;
    }
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.Pages.IgnoreAutoIncludes();
  }
}