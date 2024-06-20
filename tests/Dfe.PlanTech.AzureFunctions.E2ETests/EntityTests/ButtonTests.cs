
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class ButtonTests : EntityTests<Button, ButtonDbEntity, ButtonGenerator>
{
    protected override ButtonGenerator CreateEntityGenerator() => new();

    protected override void ClearDatabase()
    {
        var dbButtons = GetDbEntitiesQuery().ToList();

        Db.Buttons.RemoveRange(dbButtons);

        Db.SaveChanges();
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(Button entity)
     => new()
     {
         ["isStartButton"] = entity.IsStartButton,
         ["value"] = entity.Value,
     };

    protected override IQueryable<ButtonDbEntity> GetDbEntitiesQuery() => Db.Buttons.IgnoreQueryFilters().IgnoreAutoIncludes();

    protected override void ValidateDbMatches(Button entity, ButtonDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);
        Assert.Equal(entity.Value, dbEntity.Value);
        Assert.Equal(entity.IsStartButton, dbEntity.IsStartButton);

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}