
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class HeaderTests : EntityTests<Header, HeaderDbEntity, HeaderGenerator>
{
    protected override HeaderGenerator CreateEntityGenerator() => new();

    protected override void ClearDatabase()
    {
        var dbHeaders = GetDbEntitiesQuery().ToList();

        Db.Headers.RemoveRange(dbHeaders);

        Db.SaveChanges();
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(Header entity)
     => new()
     {
         ["text"] = entity.Text,
         ["tag"] = entity.Tag,
         ["size"] = entity.Size
     };

    protected override IQueryable<HeaderDbEntity> GetDbEntitiesQuery() => Db.Headers.IgnoreQueryFilters().IgnoreAutoIncludes().AsNoTracking();

    protected override void ValidateDbMatches(Header entity, HeaderDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);

        Assert.Equal(entity.Text, dbEntity.Text);
        Assert.Equal(entity.Tag, dbEntity.Tag);
        Assert.Equal(entity.Size, dbEntity.Size);

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}