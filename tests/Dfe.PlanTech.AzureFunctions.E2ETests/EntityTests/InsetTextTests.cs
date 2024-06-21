
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class InsetTextTests : EntityTests<InsetText, InsetTextDbEntity, InsetTextGenerator>
{
    protected override InsetTextGenerator CreateEntityGenerator() => new();

    protected override void ClearDatabase()
    {
        var dbInsetTexts = GetDbEntitiesQuery().ToList();

        Db.InsetTexts.RemoveRange(dbInsetTexts);

        Db.SaveChanges();
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(InsetText entity)
     => new()
     {
         ["text"] = entity.Text,
     };

    protected override IQueryable<InsetTextDbEntity> GetDbEntitiesQuery() => Db.InsetTexts.IgnoreQueryFilters().IgnoreAutoIncludes().AsNoTracking();

    protected override void ValidateDbMatches(InsetText entity, InsetTextDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);
        Assert.Equal(entity.Text, dbEntity.Text);

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}