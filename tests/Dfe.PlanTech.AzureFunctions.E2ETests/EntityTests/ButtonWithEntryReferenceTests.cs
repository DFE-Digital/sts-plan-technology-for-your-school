
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class ButtonWithEntryReferenceTests : EntityTests<ButtonWithEntryReference, ButtonWithEntryReferenceDbEntity, ButtonWithEntryReferenceGenerator>
{
  protected override ButtonWithEntryReferenceGenerator CreateEntityGenerator()
  {
    var buttonGenerator = new ButtonGenerator();

    var buttons = buttonGenerator.Generate(200);
    Db.Buttons.AddRange(buttons.Select(button => new ButtonDbEntity() { Id = button.Sys.Id, Published = true, Value = button.Value }));
    Db.SaveChanges();

    var pageGenerator = new PageGenerator([]);

    var pages = pageGenerator.Generate(100);
    var pageDbEntities = PageGenerator.MapToDbEntity(pages);
    Db.Pages.AddRange(pageDbEntities);
    Db.SaveChanges();

    return new ButtonWithEntryReferenceGenerator(buttons, pages);
  }

  protected override void ClearDatabase()
  {
    var buttons = Db.Buttons.IgnoreAutoIncludes().IgnoreQueryFilters().ToList();
    Db.Buttons.RemoveRange(buttons);

    var dbButtonWithEntryReferences = GetDbEntitiesQuery().ToList();
    Db.ButtonWithEntryReferences.RemoveRange(dbButtonWithEntryReferences);

    Db.SaveChanges();
  }

  protected override Dictionary<string, object?> CreateEntityValuesDictionary(ButtonWithEntryReference entity)
   => new()
   {
     ["button"] = new { Sys = new { entity.Button.Sys.Id } },
     ["linkToEntry"] = new
     {
       Sys = new { entity.LinkToEntry.Sys.Id }
     }
   };

  protected override IQueryable<ButtonWithEntryReferenceDbEntity> GetDbEntitiesQuery() => Db.ButtonWithEntryReferences.IgnoreAutoIncludes().IgnoreQueryFilters();

  protected override void ValidateDbMatches(ButtonWithEntryReference entity, ButtonWithEntryReferenceDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
  {
    Assert.NotNull(dbEntity);
    Assert.Equal(entity.Button.Sys.Id, dbEntity.ButtonId);
    Assert.Equal(entity.LinkToEntry.Sys.Id, dbEntity.LinkToEntryId);

    ValidateEntityState(dbEntity, published, archived, deleted);
  }
}