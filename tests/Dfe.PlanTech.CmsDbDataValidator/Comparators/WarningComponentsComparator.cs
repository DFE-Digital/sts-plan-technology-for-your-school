using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class WarningComponentComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, [], "WarningComponent")
{
  public override Task ValidateContent()
  {
    ValidateWarningComponents(_dbEntities.OfType<WarningComponentDbEntity>().ToArray());
    return Task.CompletedTask;
  }

  private void ValidateWarningComponents(WarningComponentDbEntity[] warningComponents)
  {
    foreach (var contentfulWarningComponent in _contentfulEntities)
    {
      ValidateWarningComponent(warningComponents, contentfulWarningComponent);
    }
  }

  private void ValidateWarningComponent(WarningComponentDbEntity[] warningComponents, JsonNode contentfulWarningComponent)
  {
    var contentfulWarningComponentId = contentfulWarningComponent.GetEntryId();

    if (contentfulWarningComponentId == null)
    {
      Console.WriteLine($"Couldn't find ID for Contentful warningComponent {contentfulWarningComponent}");
      return;
    }

    var matchingDbWarningComponent = warningComponents.FirstOrDefault(warningComponent => warningComponent.Id == contentfulWarningComponentId);

    if (matchingDbWarningComponent == null)
    {
      Console.WriteLine($"No matching warningComponent found for contentful warningComponent with ID: {contentfulWarningComponentId}");
      return;
    }

    var textValidationResult = ValidateChild<WarningComponentDbEntity>(matchingDbWarningComponent, "TextId", contentfulWarningComponent, "text");
    ValidateProperties(contentfulWarningComponent, matchingDbWarningComponent, textValidationResult);
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.Warnings;
  }
}