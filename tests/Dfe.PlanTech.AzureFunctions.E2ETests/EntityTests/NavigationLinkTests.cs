
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class NavigationLinkTests : EntityTests<NavigationLink, NavigationLinkDbEntity, NavigationLinkGenerator>
{
    protected override NavigationLinkGenerator CreateEntityGenerator() => new();

    protected override void ClearDatabase()
    {
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[NavigationLink]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[ContentComponents]");
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(NavigationLink entity)
     => new()
     {
         ["displayText"] = entity.DisplayText,
         ["href"] = entity.Href,
         ["openInNewTab"] = entity.OpenInNewTab,
     };

    protected override IQueryable<NavigationLinkDbEntity> GetDbEntitiesQuery() => Db.NavigationLink.IgnoreQueryFilters().IgnoreAutoIncludes().AsNoTracking();

    protected override void ValidateDbMatches(NavigationLink entity, NavigationLinkDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);
        Assert.Equal(entity.DisplayText, dbEntity.DisplayText);
        Assert.Equal(entity.Href, dbEntity.Href);
        Assert.Equal(entity.OpenInNewTab, dbEntity.OpenInNewTab);

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}