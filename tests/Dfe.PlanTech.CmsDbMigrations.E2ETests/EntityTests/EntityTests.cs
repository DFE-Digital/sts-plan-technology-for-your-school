using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Persistence.Commands;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;
using Dfe.PlanTech.CmsDbMigrations.E2ETests.Utilities;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Infrastructure.ServiceBus.Retry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.EntityTests;

public abstract class EntityTests<TEntity, TDbEntity, TEntityGenerator>
  where TEntity : ContentComponent
  where TDbEntity : ContentComponentDbEntity
  where TEntityGenerator : BaseGenerator<TEntity>
{
    public EntityTests(int listSize = 50)
    {
        CreatedEntities = new(listSize);
        var serviceProvider = Startup.CreateServiceProvider();

        Db = serviceProvider.GetRequiredService<CmsDbContext>();
        Db.ChangeTracker.Clear();

        DatabaseHelper = serviceProvider.GetRequiredService<IDatabaseHelper<ICmsDbContext>>();

        LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        Logger = LoggerFactory.CreateLogger(GetType());
        WebhookLogger = LoggerFactory.CreateLogger<WebhookToDbCommand>();
        Mappers = serviceProvider.GetRequiredService<JsonToEntityMappers>();
        MessageRetryHandler = serviceProvider.GetService<IMessageRetryHandler>() ?? EntityTests<TEntity, TDbEntity, TEntityGenerator>.CreateMessageRetryHandlerSub();
        cacheClearer = Substitute.For<ICacheClearer>();
        WebhookToDbCommand = CreateWebhookToDbCommand();

        ClearDatabase();

        EntityGenerator = CreateEntityGenerator();

    }

    protected WebhookToDbCommand WebhookToDbCommand;
    protected readonly CmsDbContext Db;
    protected readonly ILogger Logger;
    protected readonly ILoggerFactory LoggerFactory;
    protected readonly IMessageRetryHandler MessageRetryHandler;
    protected readonly JsonToEntityMappers Mappers;
    protected readonly List<TEntity> CreatedEntities;
    protected readonly TEntityGenerator EntityGenerator;
    protected readonly ICacheClearer cacheClearer;
    protected readonly IDatabaseHelper<ICmsDbContext> DatabaseHelper;
    protected readonly ILogger<WebhookToDbCommand> WebhookLogger;

    [Fact]
    public async Task Should_Publish_Entities()
    {
        await CreateAndPublishEntities();

        await ValidateAllEntitiesMatch(CreatedEntities);

        ClearDatabase();
    }

    [Fact]
    public async Task Should_Update_Db_On_Publish()
    {
        await CreateAndPublishEntities();

        var cmsEvent = CmsEvent.PUBLISH;

        var updatedEntities = EntityGenerator.CopyWithDifferentValues(CreatedEntities).ToList();

        foreach (var entity in updatedEntities)
        {
            await WebhookToDbCommand.ProcessMessage(cmsEvent.ToString(), CreateWebhookBody(entity.Updated), entity.Original.Sys.Id, default);

            var dbEntity = await GetDbEntityById(entity.Original.Sys.Id);
            ValidateDbMatches(entity.Updated, dbEntity, published: true);
        }

        await ValidateAllEntitiesMatch(updatedEntities.Select(updatedEntity => updatedEntity.Updated), true, false, false);
        ClearDatabase();
    }

    [Fact]
    public async Task Should_Ignore_Draft_Changes_When_UsePreview_False()
    {
        await CreateAndPublishEntities();

        var cmsEvent = CmsEvent.SAVE;

        foreach (var entity in CreatedEntities)
        {
            var newEntity = EntityGenerator.CopyWithDifferentValues(entity);

            await WebhookToDbCommand.ProcessMessage(cmsEvent.ToString(), CreateWebhookBody(entity), entity.Sys.Id, default);

            var dbEntity = await GetDbEntityById(entity.Sys.Id);
            ValidateDbMatches(entity, dbEntity, published: true);
        }

        await ValidateAllEntitiesMatch(CreatedEntities, true, false, false);
        ClearDatabase();
    }

    [Fact]
    public async Task Should_Unpublish_Entities()
    {
        await CreateAndPublishEntities();

        var cmsEvent = CmsEvent.UNPUBLISH;
        foreach (var entity in CreatedEntities)
        {
            await UpdateEntityStatus(entity, cmsEvent);

            var dbEntity = await GetDbEntityById(entity.Sys.Id);
            ValidateDbMatches(entity, dbEntity, published: false);
        }

        await ValidateAllEntitiesMatch(CreatedEntities, false, false, false);
        ClearDatabase();
    }

    [Fact]
    public async Task Should_Save_Changes_When_Draft_Content_Allowed()
    {
        WebhookToDbCommand = CreateWebhookToDbCommand(true);
        var cmsEvent = CmsEvent.SAVE;

        await CreateAndPublishEntities();

        var updatedEntities = EntityGenerator.CopyWithDifferentValues(CreatedEntities).ToList();

        foreach (var entity in updatedEntities)
        {
            await WebhookToDbCommand.ProcessMessage(cmsEvent.ToString(), CreateWebhookBody(entity.Updated), entity.Original.Sys.Id, default);

            var dbEntity = await GetDbEntityById(entity.Original.Sys.Id);
            ValidateDbMatches(entity.Updated, dbEntity, published: true);
        }

        await ValidateAllEntitiesMatch(updatedEntities.Select(entity => entity.Updated), true, false, false);
        ClearDatabase();
    }

    [Fact]
    public async Task Should_Republish_Content()
    {
        await CreateAndPublishEntities();

        foreach (var entity in CreatedEntities)
        {
            await UpdateEntityStatus(entity, CmsEvent.UNPUBLISH);
        }

        var cmsEvent = CmsEvent.PUBLISH;

        var updatedEntities = EntityGenerator.CopyWithDifferentValues(CreatedEntities).ToList();

        foreach (var entity in updatedEntities)
        {
            await WebhookToDbCommand.ProcessMessage(cmsEvent.ToString(), CreateWebhookBody(entity.Updated), entity.Original.Sys.Id, default);

            var dbEntity = await GetDbEntityById(entity.Original.Sys.Id);
            ValidateDbMatches(entity.Updated, dbEntity, published: true);
        }

        await ValidateAllEntitiesMatch(updatedEntities.Select(entity => entity.Updated), true, false, false);
        ClearDatabase();
    }

    [Fact]
    public async Task Should_Archive_Content()
    {
        await CreateAndPublishEntities();

        var cmsEvent = CmsEvent.ARCHIVE;

        var updatedEntities = EntityGenerator.CopyWithDifferentValues(CreatedEntities).ToList();

        foreach (var entity in updatedEntities)
        {
            await WebhookToDbCommand.ProcessMessage(cmsEvent.ToString(), CreateWebhookBody(entity.Updated), entity.Original.Sys.Id, default);

            var dbEntity = await GetDbEntityById(entity.Original.Sys.Id);
            ValidateDbMatches(entity.Updated, dbEntity, published: true, archived: true);
        }

        await ValidateAllEntitiesMatch(updatedEntities.Select(entity => entity.Updated), true, true, false);
        ClearDatabase();
    }

    [Fact]
    public async Task Should_Unarchive_Content()
    {
        await CreateAndPublishEntities();

        var updatedEntities = EntityGenerator.CopyWithDifferentValues(CreatedEntities).ToList();

        foreach (var entity in updatedEntities)
        {
            Db.ChangeTracker.Clear();

            await WebhookToDbCommand.ProcessMessage(CmsEvent.ARCHIVE.ToString(), CreateWebhookBody(entity.Updated), entity.Original.Sys.Id, default);

            Db.ChangeTracker.Clear();

            await WebhookToDbCommand.ProcessMessage(CmsEvent.UNARCHIVE.ToString(), CreateWebhookBody(entity.Updated), entity.Original.Sys.Id, default);

            Db.ChangeTracker.Clear();

            var dbEntity = await GetDbEntityById(entity.Original.Sys.Id);
            ValidateDbMatches(entity.Updated, dbEntity, published: true, archived: false);
        }

        await ValidateAllEntitiesMatch(updatedEntities.Select(entity => entity.Updated), true, false, false);
        ClearDatabase();
    }

    [Fact]
    public async Task Should_Delete_Content()
    {
        await CreateAndPublishEntities();

        foreach (var entity in CreatedEntities)
        {
            await UpdateEntityStatus(entity, CmsEvent.DELETE);

            var dbEntity = await GetDbEntityById(entity.Sys.Id);
            ValidateDbMatches(entity, dbEntity, published: true, archived: false, deleted: true);
        }

        ClearDatabase();
    }

    [Fact]
    public async Task Should_Allow_Save_When_NotExisting_In_Db()
    {
        var entities = EntityGenerator.Generate(20);

        CreatedEntities.AddRange(entities);

        var cmsEvent = CmsEvent.SAVE;

        foreach (var entity in entities)
        {
            await WebhookToDbCommand.ProcessMessage(cmsEvent.ToString(), CreateWebhookBody(entity), entity.Sys.Id, default);

            var dbEntity = await GetDbEntityById(entity.Sys.Id);
            ValidateDbMatches(entity, dbEntity, published: false);
        }

        Db.ChangeTracker.Clear();

        await ValidateAllEntitiesMatch(entities, false, false, false);
        ClearDatabase();
    }

    [Fact]
    public async Task Should_Allow_AutoSave_When_NotExisting_In_Db()
    {
        var entities = EntityGenerator.Generate(20);

        CreatedEntities.AddRange(entities);

        var cmsEvent = CmsEvent.AUTO_SAVE;

        foreach (var entity in entities)
        {
            await WebhookToDbCommand.ProcessMessage(cmsEvent.ToString(), CreateWebhookBody(entity), entity.Sys.Id, default);

            Db.ChangeTracker.Clear();

            var dbEntity = await GetDbEntityById(entity.Sys.Id);
            ValidateDbMatches(entity, dbEntity, published: false);
        }

        Db.ChangeTracker.Clear();

        await ValidateAllEntitiesMatch(entities, false, false, false);
        ClearDatabase();
    }

    [Fact]
    public async Task Should_Allow_Save_When_Exists_But_Not_Published()
    {
        await CreateAndPublishEntities();

        var updatedEntities = EntityGenerator.CopyWithDifferentValues(CreatedEntities).ToList();

        foreach (var entity in updatedEntities)
        {
            Db.ChangeTracker.Clear();

            //Unpublish entity first
            await UpdateEntityStatus(entity.Original, CmsEvent.UNPUBLISH);

            Db.ChangeTracker.Clear();

            //Update with SAVE event
            await WebhookToDbCommand.ProcessMessage(CmsEvent.SAVE.ToString(), CreateWebhookBody(entity.Updated), entity.Updated.Sys.Id, default);

            //Assert changed
            var dbEntity = await GetDbEntityById(entity.Original.Sys.Id);
            ValidateDbMatches(entity.Updated, dbEntity, published: false);
        }

        await ValidateAllEntitiesMatch(updatedEntities.Select(entity => entity.Updated), false, false, false);
        ClearDatabase();
    }

    [Fact]
    public async Task Should_Allow_AutoSave_When_Exists_But_Not_Published()
    {
        await CreateAndPublishEntities();

        var updatedEntities = EntityGenerator.CopyWithDifferentValues(CreatedEntities).ToList();

        foreach (var entity in updatedEntities)
        {
            Db.ChangeTracker.Clear();

            //Unpublish entity first
            await UpdateEntityStatus(entity.Original, CmsEvent.UNPUBLISH);

            Db.ChangeTracker.Clear();

            //Update with SAVE event
            await WebhookToDbCommand.ProcessMessage(CmsEvent.AUTO_SAVE.ToString(), CreateWebhookBody(entity.Updated), entity.Updated.Sys.Id, default);

            //Assert changed
            var dbEntity = await GetDbEntityById(entity.Original.Sys.Id);
            ValidateDbMatches(entity.Updated, dbEntity, published: false);
        }

        await ValidateAllEntitiesMatch(updatedEntities.Select(entity => entity.Updated), false, false, false);
        ClearDatabase();
    }


    private async Task UpdateEntityStatus(TEntity entity, CmsEvent cmsEvent)
    {
        await WebhookToDbCommand.ProcessMessage(cmsEvent.ToString(), CreateWebhookBody(entity), entity.Sys.Id, default);
        Db.ChangeTracker.Clear();
    }

    private async Task ValidateAllEntitiesMatch(IEnumerable<TEntity> entities, bool published = true, bool archived = false, bool deleted = false)
    {
        Db.ChangeTracker.Clear();

        foreach (var entity in entities)
        {
            var matchingDbEntity = await GetDbEntityById(entity);
            ValidateDbMatches(entity, matchingDbEntity, published, archived, deleted);
        }

        Db.ChangeTracker.Clear();
    }

    private async Task CreateAndPublishEntities(Func<TEntity, Task>? afterEntitySavedCallback = null)
    {
        var entities = EntityGenerator.Generate(20);

        CreatedEntities.AddRange(entities);

        var cmsEvent = CmsEvent.PUBLISH;

        foreach (var entity in entities)
        {
            await WebhookToDbCommand.ProcessMessage(cmsEvent.ToString(), CreateWebhookBody(entity), entity.Sys.Id, default);

            Db.ChangeTracker.Clear();

            var dbEntity = await GetDbEntityById(entity.Sys.Id);
            ValidateDbMatches(entity, dbEntity, published: true);

            afterEntitySavedCallback?.Invoke(entity);
        }

        Db.ChangeTracker.Clear();
    }

    protected WebhookToDbCommand CreateWebhookToDbCommand(bool usePreviewContent = false)
    => new(cacheClearer, new ContentfulOptions(usePreviewContent), Mappers, WebhookLogger, DatabaseHelper);

    protected virtual Task<TDbEntity?> GetDbEntityById(TEntity entity) => GetDbEntityById(entity.Sys.Id);

    protected virtual Task<TDbEntity?> GetDbEntityById(string id) => GetDbEntitiesQuery().AsNoTracking().FirstOrDefaultAsync(dbEntity => dbEntity.Id == id);

    protected virtual void ValidateEntityState(TDbEntity dbEntity, bool published, bool archived, bool deleted)
    {
        Assert.Equal(published, dbEntity.Published);
        Assert.Equal(archived, dbEntity.Archived);
        Assert.Equal(deleted, dbEntity.Deleted);
    }

    protected virtual void ClearDatabase()
    {
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[PageContents]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationSectionAnswers]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationSectionChunks]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationSections]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationIntroContents]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationIntros]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationChunkContents]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationChunkAnswers]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationChunks]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[ButtonWithEntryReferences]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Answers]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Questions]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[NavigationLink]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Sections]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Categories]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Headers]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[ButtonWithLinks]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Buttons]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[InsetTexts]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Warnings]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[TextBodies]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Pages]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Titles]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RichTextMarkDbEntity]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RichTextContents]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RichTextDataDbEntity]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[ContentComponents]");
    }
    protected abstract TEntityGenerator CreateEntityGenerator();
    protected abstract Dictionary<string, object?> CreateEntityValuesDictionary(TEntity entity);
    protected abstract void ValidateDbMatches(TEntity entity, TDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false);

    protected abstract IQueryable<TDbEntity> GetDbEntitiesQuery();

    private static IMessageRetryHandler CreateMessageRetryHandlerSub()
    {
        var sub = Substitute.For<IMessageRetryHandler>();
        sub.RetryRequired(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>()).Returns((callinfo) => Task.FromResult(false));
        return sub;
    }

    private string CreateWebhookBody(TEntity entity)
    {
        var dictionary = CreateEntityValuesDictionary(entity);

        return EntityToPayload.ConvertEntityToPayload(entity, dictionary);
    }
}
