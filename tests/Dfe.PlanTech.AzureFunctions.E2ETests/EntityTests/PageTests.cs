
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class PageTests() : EntityTests<Page, PageDbEntity, PageGenerator>
{
    protected override PageGenerator CreateEntityGenerator() => PageGenerator.CreateInstance(Db);

    protected override void ClearDatabase()
    {
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[PageContents]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Pages]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Titles]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[ContentComponents]");
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(Page entity)
        => new()
        {
            ["beforeTitleContent"] = entity.BeforeTitleContent.Select(content => new { Sys = new { content.Sys.Id } }),
            ["content"] = entity.Content.Select(content => new { Sys = new { content.Sys.Id } }),
            ["displayBackButton"] = entity.DisplayBackButton,
            ["displayHomeButton"] = entity.DisplayHomeButton,
            ["displayOrganisationName"] = entity.DisplayOrganisationName,
            ["displayTopicTitle"] = entity.DisplayTopicTitle,
            ["internalName"] = entity.InternalName,
            ["slug"] = entity.Slug,
            ["title"] = entity.Title != null ? new { Sys = new { entity.Title?.Sys.Id } } : null
        };

    protected override IQueryable<PageDbEntity> GetDbEntitiesQuery() => Db.Pages.IgnoreAutoIncludes().IgnoreQueryFilters();

    protected override void ValidateDbMatches(Page entity, PageDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);

        Assert.Equal(entity.DisplayBackButton, dbEntity.DisplayBackButton);
        Assert.Equal(entity.DisplayHomeButton, dbEntity.DisplayHomeButton);
        Assert.Equal(entity.DisplayOrganisationName, dbEntity.DisplayOrganisationName);
        Assert.Equal(entity.DisplayTopicTitle, dbEntity.DisplayTopicTitle);
        Assert.Equal(entity.InternalName, dbEntity.InternalName);
        Assert.Equal(entity.Title?.Sys.Id, dbEntity.TitleId);

        ValidateEntityState(dbEntity, published, archived, deleted);

        var pageContentComponents = Db.PageContents.Where(contentComponent => contentComponent.PageId == entity.Sys.Id).ToList();

        var beforeTitleContent = pageContentComponents.Where(cc => cc.BeforeContentComponentId != null).ToArray();
        Assert.Equal(entity.BeforeTitleContent.Count, beforeTitleContent.Length);

        foreach (var contentComponent in entity.BeforeTitleContent)
        {
            var matchingContentComponent = beforeTitleContent.Where(btc => btc.BeforeContentComponentId == contentComponent.Sys.Id).FirstOrDefault();
            Assert.NotNull(matchingContentComponent);
        }

        var content = pageContentComponents.Where(cc => cc.ContentComponentId != null).ToArray();
        Assert.Equal(entity.Content.Count, content.Length);

        foreach (var contentComponent in entity.Content)
        {
            var matchingContentComponent = content.Where(btc => btc.ContentComponentId == contentComponent.Sys.Id).FirstOrDefault();
            Assert.NotNull(matchingContentComponent);
        }
    }
}