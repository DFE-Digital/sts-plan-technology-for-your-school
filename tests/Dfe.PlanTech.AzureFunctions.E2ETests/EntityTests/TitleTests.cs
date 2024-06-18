
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.Plantech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class TitleTests() : EntityTests<Title, TitleDbEntity, TitleGenerator>
{
  protected override TitleGenerator CreateEntityGenerator() => new();

  protected override void ClearDatabase()
  {
    var titles = GetDbEntitiesQuery().ToList();
    Db.Titles.RemoveRange(titles);
    Db.SaveChanges();
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

    Assert.Equal(entity.Text, entity.Text);

    ValidateEntityState(dbEntity, published, archived, deleted);
  }
}