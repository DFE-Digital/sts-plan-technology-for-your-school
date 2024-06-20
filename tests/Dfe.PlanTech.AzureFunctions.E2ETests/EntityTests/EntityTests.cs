using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions;
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.AzureFunctions.E2ETests.Utilities;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.Utils;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

public abstract class EntityTests<TEntity, TDbEntity, TEntityGenerator>
  where TEntity : ContentComponent
  where TDbEntity : ContentComponentDbEntity
  where TEntityGenerator : BaseGenerator<TEntity>
{
    public EntityTests(int listSize = 50)
    {
        CreatedEntities = new(listSize);
        var serviceProvider = PlanTech.AzureFunctions.E2ETests.Startup.CreateServiceProvider();

        Db = serviceProvider.GetRequiredService<CmsDbContext>();
        Db.ChangeTracker.Clear();

        LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        Logger = LoggerFactory.CreateLogger(GetType());
        Mappers = serviceProvider.GetRequiredService<JsonToEntityMappers>();
        MessageActions = CreateServiceBusMessageActionsSubstitute();
        MessageRetryHandler = serviceProvider.GetService<IMessageRetryHandler>() ?? EntityTests<TEntity, TDbEntity, TEntityGenerator>.CreateMessageRetryHandlerSub();
        QueueReceiver = CreateQueueReceiver();

        ClearDatabase();

        EntityGenerator = CreateEntityGenerator();
    }

    protected QueueReceiver QueueReceiver;
    protected readonly CmsDbContext Db;
    protected readonly ILogger Logger;
    protected readonly ILoggerFactory LoggerFactory;
    protected readonly IMessageRetryHandler MessageRetryHandler;
    protected readonly JsonToEntityMappers Mappers;
    protected readonly List<TEntity> CreatedEntities;
    protected readonly ServiceBusMessageActions MessageActions;
    protected readonly TEntityGenerator EntityGenerator;

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
            var message = CreateServiceBusMessage(entity.Updated, cmsEvent);
            await QueueReceiver.QueueReceiverDbWriter([message], MessageActions, default);

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
            var message = CreateServiceBusMessage(newEntity, cmsEvent);

            await QueueReceiver.QueueReceiverDbWriter([message], MessageActions, default);

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
        QueueReceiver = CreateQueueReceiver(true);
        var cmsEvent = CmsEvent.SAVE;

        await CreateAndPublishEntities();

        var updatedEntities = EntityGenerator.CopyWithDifferentValues(CreatedEntities).ToList();

        foreach (var entity in updatedEntities)
        {
            var message = CreateServiceBusMessage(entity.Updated, cmsEvent);
            await QueueReceiver.QueueReceiverDbWriter([message], MessageActions, default);

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
            var message = CreateServiceBusMessage(entity.Updated, cmsEvent);
            await QueueReceiver.QueueReceiverDbWriter([message], MessageActions, default);

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
            var message = CreateServiceBusMessage(entity.Updated, cmsEvent);
            await QueueReceiver.QueueReceiverDbWriter([message], MessageActions, default);

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

            var archiveMessage = CreateServiceBusMessage(entity.Updated, CmsEvent.ARCHIVE);
            await QueueReceiver.QueueReceiverDbWriter([archiveMessage], MessageActions, default);

            Db.ChangeTracker.Clear();

            var unarchiveMessage = CreateServiceBusMessage(entity.Updated, CmsEvent.UNARCHIVE);
            await QueueReceiver.QueueReceiverDbWriter([unarchiveMessage], MessageActions, default);

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

    private async Task UpdateEntityStatus(TEntity entity, CmsEvent cmsEvent)
    {
        var message = CreateBlankMessage(entity, cmsEvent);
        await QueueReceiver.QueueReceiverDbWriter([message], MessageActions, default);
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
            var message = CreateServiceBusMessage(entity, cmsEvent);
            await QueueReceiver.QueueReceiverDbWriter([message], MessageActions, default);

            var dbEntity = await GetDbEntityById(entity.Sys.Id);
            ValidateDbMatches(entity, dbEntity, published: true);

            afterEntitySavedCallback?.Invoke(entity);
        }

        Db.ChangeTracker.Clear();
    }

    protected QueueReceiver CreateQueueReceiver(bool usePreviewContent = false)
    => new(new ContentfulOptions(usePreviewContent), LoggerFactory, Db, Mappers, MessageRetryHandler);

    protected ServiceBusReceivedMessage CreateBlankMessage(TEntity entity, CmsEvent cmsEvent)
    {
        var dictionary = new Dictionary<string, object?>()
        {
        };

        return EntityToPayload.CreateServiceBusMessage(entity, dictionary, cmsEvent, Logger);
    }

    protected ServiceBusReceivedMessage CreateServiceBusMessage(TEntity entity, CmsEvent cmsEvent)
    {
        var dictionary = CreateEntityValuesDictionary(entity);

        return EntityToPayload.CreateServiceBusMessage(entity, dictionary, cmsEvent, Logger);
    }

    protected virtual Task<TDbEntity?> GetDbEntityById(TEntity entity) => GetDbEntityById(entity.Sys.Id);
    protected virtual Task<TDbEntity?> GetDbEntityById(string id) => GetDbEntitiesQuery().AsNoTracking().FirstOrDefaultAsync(dbEntity => dbEntity.Id == id);

    protected virtual void ValidateEntityState(TDbEntity dbEntity, bool published, bool archived, bool deleted)
    {
        Assert.Equal(published, dbEntity.Published);
        Assert.Equal(archived, dbEntity.Archived);
        Assert.Equal(deleted, dbEntity.Deleted);
    }

    protected abstract void ClearDatabase();
    protected abstract TEntityGenerator CreateEntityGenerator();
    protected abstract Dictionary<string, object?> CreateEntityValuesDictionary(TEntity entity);
    protected abstract void ValidateDbMatches(TEntity entity, TDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false);
    protected abstract IQueryable<TDbEntity> GetDbEntitiesQuery();

    private static ServiceBusMessageActions CreateServiceBusMessageActionsSubstitute()
    {
        var sub = Substitute.For<ServiceBusMessageActions>();
        sub.CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>())
            .Returns((CallInfo) =>
            {
                return Task.CompletedTask;
            });

        sub.DeadLetterMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<Dictionary<string, object>?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
        .Returns((CallInfo) =>
        {
            return Task.CompletedTask;
        });

        return sub;
    }

    private static IMessageRetryHandler CreateMessageRetryHandlerSub()
    {
        var sub = Substitute.For<IMessageRetryHandler>();
        sub.RetryRequired(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>()).Returns((callinfo) => Task.FromResult(false));
        return sub;
    }

}