
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class ButtonWithLinkTests : EntityTests<ButtonWithLink, ButtonWithLinkDbEntity, ButtonWithLinkGenerator>
{
    protected override ButtonWithLinkGenerator CreateEntityGenerator()
    {
        var buttonGenerator = new ButtonGenerator();
        var buttons = buttonGenerator.Generate(200);

        Db.Buttons.AddRange(buttons.Select(button => new ButtonDbEntity() { Id = button.Sys.Id, Published = true, Value = button.Value }));
        Db.SaveChanges();

        return new ButtonWithLinkGenerator(buttons);
    }

    protected override void ClearDatabase()
    {
        var buttons = Db.Buttons.IgnoreAutoIncludes().IgnoreQueryFilters().ToList();
        Db.Buttons.RemoveRange(buttons);

        var dbButtonWithLinks = GetDbEntitiesQuery().ToList();
        Db.ButtonWithLinks.RemoveRange(dbButtonWithLinks);

        Db.SaveChanges();
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(ButtonWithLink entity)
     => new()
     {
         ["button"] = new { Sys = new { entity.Button.Sys.Id } },
         ["href"] = entity.Href
     };

    protected override IQueryable<ButtonWithLinkDbEntity> GetDbEntitiesQuery() => Db.ButtonWithLinks.IgnoreAutoIncludes().IgnoreQueryFilters();

    protected override void ValidateDbMatches(ButtonWithLink entity, ButtonWithLinkDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);
        Assert.Equal(entity.Button.Sys.Id, dbEntity.ButtonId);
        Assert.Equal(entity.Href, dbEntity.Href);

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}