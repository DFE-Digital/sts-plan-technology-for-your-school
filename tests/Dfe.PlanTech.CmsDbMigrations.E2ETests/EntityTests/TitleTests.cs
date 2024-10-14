
using Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class TitleTests() : EntityTests<Title, TitleDbEntity, TitleGenerator>
{
    protected override TitleGenerator CreateEntityGenerator() => new();

    protected override void ClearDatabase()
    {
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[ContentComponents]");
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(Title entity)
     => new()
     {
         ["text"] = entity.Text,
     };

    protected override IQueryable<TitleDbEntity> GetDbEntitiesQuery() => Db.Titles.IgnoreQueryFilters().IgnoreAutoIncludes();

    protected override void ValidateDbMatches(Title entity, TitleDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);

        Assert.Equal(entity.Text, dbEntity.Text);

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}
