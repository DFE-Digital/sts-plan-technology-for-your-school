
using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class ButtonWithLinksComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Href"], "ButtonWithLink")
{
  public override Task ValidateContent()
  {
    ValidateButtons(_dbEntities.OfType<ButtonWithLinkDbEntity>().ToArray());
    return Task.CompletedTask;
  }

  private void ValidateButtons(ButtonWithLinkDbEntity[] buttonWithLinks)
  {
    foreach (var contentfulButton in _contentfulEntities)
    {
      ValidateButton(buttonWithLinks, contentfulButton);
    }
  }

  private void ValidateButton(ButtonWithLinkDbEntity[] ButtonWithLinks, JsonNode contentfulButton)
  {
    var databaseButton = ValidateChildEntityExistsInDb(ButtonWithLinks, contentfulButton);
    if (databaseButton == null)
    {
      return;
    }

    CompareStrings("ButtonId", contentfulButton["button"]?.GetEntryId(), databaseButton.ButtonId);
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.ButtonWithLinks;
  }
}