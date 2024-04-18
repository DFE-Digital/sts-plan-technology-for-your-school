using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class TitleComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Text"], "Title")
{
  public override Task ValidateContent()
  {
    ValidateTitles(_dbEntities.OfType<TitleDbEntity>().ToArray());
    return Task.CompletedTask;
  }

  private void ValidateTitles(TitleDbEntity[] titles)
  {
    foreach (var contentfulTitle in _contentfulEntities)
    {
      ValidateTitle(titles, contentfulTitle);
    }
  }

  private void ValidateTitle(TitleDbEntity[] titles, JsonNode contentfulTitle)
  {
    var contentfulTitleId = contentfulTitle.GetEntryId();

    if (contentfulTitleId == null)
    {
      Console.WriteLine($"Couldn't find ID for Contentful title {contentfulTitle}");
      return;
    }

    var matchingDbTitle = titles.FirstOrDefault(title => title.Id == contentfulTitleId);

    if (matchingDbTitle == null)
    {
      Console.WriteLine($"No matching title found for contentful title with ID: {contentfulTitleId}");
      return;
    }

    ValidateProperties(contentfulTitle, matchingDbTitle);
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.Titles;
  }
}