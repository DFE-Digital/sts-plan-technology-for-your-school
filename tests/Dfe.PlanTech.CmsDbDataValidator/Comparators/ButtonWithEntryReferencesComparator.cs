using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class ButtonWithEntryReferencesComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, [], "ButtonWithLink")
{
  public override Task ValidateContent()
  {
    ValidateButtons(_dbEntities.OfType<ButtonWithEntryReferenceDbEntity>().ToArray());
    return Task.CompletedTask;
  }

  private void ValidateButtons(ButtonWithEntryReferenceDbEntity[] buttonWithEntryReferences)
  {
    foreach (var contentfulButton in _contentfulEntities)
    {
      ValidateButton(buttonWithEntryReferences, contentfulButton);
    }
  }

  private void ValidateButton(ButtonWithEntryReferenceDbEntity[] buttonWithEntryReferences, JsonNode contentfulButton)
  {
    var databaseButton = FindMatchingDbEntity(buttonWithEntryReferences, contentfulButton);
    if (databaseButton == null)
    {
      return;
    }

    CompareStrings("ButtonId", GetId(contentfulButton["button"])!, databaseButton.ButtonId);
    CompareStrings("LinkToEntryId", GetId(contentfulButton["linkToEntry"])!, databaseButton.LinkToEntryId);
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.ButtonWithEntryReferences;
  }
}