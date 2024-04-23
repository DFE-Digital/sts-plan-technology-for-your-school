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

    foreach (var child in ValidateChildren(contentfulPage, "content", dbPage, (page) => page.Content))
    {
      yield return child;
    }

    foreach (var child in ValidateChildren(contentfulPage, "beforeTitleContent", dbPage, (page) => page.BeforeTitleContent))
    {
      yield return child;
    }
  }

  protected override Task<bool> GetDbEntities()
  {
    return GetDbEntitiesPaginated(10);
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.Pages.Select(page => new PageDbEntity()
    {
      Id = page.Id,
      InternalName = page.InternalName,
      Slug = page.Slug,
      DisplayBackButton = page.DisplayBackButton,
      DisplayHomeButton = page.DisplayHomeButton,
      DisplayOrganisationName = page.DisplayOrganisationName,
      DisplayTopicTitle = page.DisplayTopicTitle,
      TitleId = page.TitleId,
      Content = page.Content.Select(content => new ContentComponentDbEntity() { Id = content.Id })
                            .ToList(),
      BeforeTitleContent = page.BeforeTitleContentPagesJoins.Select(join => new ContentComponentDbEntity() { Id = join.BeforeContentComponentId! })
                                                            .ToList(),
    });
  }
}