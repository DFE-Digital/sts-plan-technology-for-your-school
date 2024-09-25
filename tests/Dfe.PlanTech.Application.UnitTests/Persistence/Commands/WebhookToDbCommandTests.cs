using System.Text.Json;
using System.Text.Json.Serialization;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Commands;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Application.UnitTests.TestHelpers;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Commands;

public class WebhookToDbCommandTests
{
    private const string QuestionJsonBody =
        "{\"metadata\":{\"tags\":[]},\"fields\":{\"internalName\":{\"en-US\":\"TestingQuestion\"},\"text\":{\"en-US\":\"TestingQuestion\"},\"helpText\":{\"en-US\":\"HelpText\"},\"answers\":{\"en-US\":[{\"sys\":{\"type\":\"Link\",\"linkType\":\"Entry\",\"id\":\"4QscetbCYG4MUsGdoDU0C3\"}}]},\"slug\":{\"en-US\":\"testing-slug\"}},\"sys\":{\"type\":\"Entry\",\"id\":\"2VSR0emw0SPy8dlR9XlgfF\",\"space\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"Space\",\"id\":\"py5afvqdlxgo\"}},\"environment\":{\"sys\":{\"id\":\"dev\",\"type\":\"Link\",\"linkType\":\"Environment\"}},\"contentType\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"ContentType\",\"id\":\"question\"}},\"createdBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"5yhMQOCN9P2vGpfjyZKiey\"}},\"updatedBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"4hiJvkyVWdhTt6c4ZoDkMf\"}},\"revision\":13,\"createdAt\":\"2023-12-04T14:36:46.614Z\",\"updatedAt\":\"2023-12-15T16:16:45.034Z\"}}";

    private const string QuestionId = "2VSR0emw0SPy8dlR9XlgfF";

    private readonly ILogger<WebhookToDbCommand> _logger = Substitute.For<ILogger<WebhookToDbCommand>>();
    private readonly JsonToEntityMappers _jsonToEntityMappers;
    private readonly ICacheHandler _cacheHandler = Substitute.For<ICacheHandler>();
    private readonly IDatabaseHelper<ICmsDbContext> _databaseHelper = Substitute.For<IDatabaseHelper<ICmsDbContext>>();

    private readonly QuestionDbEntity _newQuestion = new()
        { Archived = false, Published = true, Deleted = false, Id = QuestionId };

    private readonly QuestionDbEntity _existingQuestion = new()
        { Archived = false, Published = true, Deleted = false, Id = "other-content-component" };

    private readonly List<QuestionDbEntity> _questions = [];
    private readonly List<AnswerDbEntity> _answers = [];

    private readonly List<ContentComponentDbEntity> _contentComponents = [];

    private readonly IWebhookToDbCommand _webhookToDbCommand;

    public WebhookToDbCommandTests()
    {
        _contentComponents.Add(_existingQuestion);
        _questions.Add(_existingQuestion);
        var database = Substitute.For<ICmsDbContext>();
        _databaseHelper.Database.Returns(database);

        _jsonToEntityMappers = CreateMappers();

        _databaseHelper.MockDatabaseCollection(_questions);
        _databaseHelper.MockDatabaseCollection(_answers);

        _databaseHelper.Database.SetComponentPublishedAndDeletedStatuses(Arg.Any<ContentComponentDbEntity>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((callinfo) =>
            {
                var contentComponent = callinfo.ArgAt<ContentComponentDbEntity>(0);
                var published = callinfo.ArgAt<bool>(1);
                var deleted = callinfo.ArgAt<bool>(2);

                contentComponent.Published = published;
                contentComponent.Deleted = deleted;

                return Task.FromResult(1);
            });

        _webhookToDbCommand = CreateWebhookToDbCommand(true);
    }

    private WebhookToDbCommand CreateWebhookToDbCommand(bool usePreview) => new(_cacheHandler,
        new ContentfulOptions(usePreview), _jsonToEntityMappers, _logger, _databaseHelper);

    private JsonToEntityMappers CreateMappers()
    {
        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        return Substitute.For<JsonToEntityMappers>(
            new JsonToDbMapper[]
            {
                new QuestionMapper(new EntityUpdater(Substitute.For<ILogger<EntityUpdater>>(), _databaseHelper),
                    Substitute.For<ILogger<QuestionMapper>>(), jsonOptions, _databaseHelper)
            }, jsonOptions);
    }

    [Fact]
    public async Task ProcessMessage_Should_Execute_Successfully()
    {
        var subject = "ContentManagement.Entry.save";

        var result =
            await _webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusSuccessResult>(result);
        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cacheHandler.Received(1).RequestCacheClear(Arg.Any<CancellationToken>());
        _databaseHelper.Received(1).Add(Arg.Any<ContentComponentDbEntity>());

        Assert.Equal(2, _questions.Count);

        var addedQuestion = _questions[1];

        var asContentComponentDbEntity = (ContentComponentDbEntity)addedQuestion;

        Assert.NotNull(asContentComponentDbEntity);
        Assert.Equal(QuestionId, asContentComponentDbEntity.Id);
    }

    [Fact]
    public async Task ProcessMessage_Should_DeadLetter_Failed_Operation()
    {
        string nonMappableJson = "\"INVALID\":\"CONTENT\"";

        var subject = "ContentManagement.Entry.save";

        var result =
            await _webhookToDbCommand.ProcessMessage(subject, nonMappableJson, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusErrorResult>(result);
    }

    [Fact]
    public async Task ProcessMessage_Should_MapExistingDbEntity_To_Message()
    {
        _questions.Add(_newQuestion);

        var subject = "ContentManagement.Entry.save";

        var result =
            await _webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusSuccessResult>(result);
        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cacheHandler.Received(1).RequestCacheClear(Arg.Any<CancellationToken>());

        _databaseHelper.ReceivedWithAnyArgs(0).Add(Arg.Any<ContentComponentDbEntity>());
        _databaseHelper.ReceivedWithAnyArgs(1).Update(Arg.Any<ContentComponentDbEntity>());
    }

    [Fact]
    public async Task ProcessMessage_Should_CompleteSuccessfully_After_Archive()
    {
        _questions.Add(_newQuestion);
        var subject = "ContentManagement.Entry.archive";

        var result =
            await _webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        _databaseHelper.Received(1).ClearTracking();
        await _cacheHandler.Received(1).RequestCacheClear(Arg.Any<CancellationToken>());
        _databaseHelper.Received(1).Update(Arg.Any<ContentComponentDbEntity>());
        await _databaseHelper.Database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        Assert.Equal(2, _questions.Count);
        var added = _questions[1];
        Assert.NotNull(added);
        Assert.True(added.Archived);
    }

    [Fact]
    public async Task ProcessMessage_Should_CompleteSuccessfully_After_Unarchive()
    {
        _questions.Add(_newQuestion);

        _existingQuestion.Archived = true;
        var subject = "ContentManagement.Entry.unarchive";

        var result =
            await _webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusSuccessResult>(result);
        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cacheHandler.Received(1).RequestCacheClear(Arg.Any<CancellationToken>());
        _databaseHelper.Received(1).Update(Arg.Any<ContentComponentDbEntity>());

        Assert.Equal(2, _questions.Count);
        var added = _questions[1];
        Assert.NotNull(added);
        Assert.False(added.Archived);
    }

    [Fact]
    public async Task ProcessMessage_Should_CompleteSuccessfully_After_Publish()
    {
        _questions.Add(_newQuestion);
        _newQuestion.Published = true;

        var subject = "ContentManagement.Entry.publish";

        var result =
            await _webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusSuccessResult>(result);
        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cacheHandler.Received(1).RequestCacheClear(Arg.Any<CancellationToken>());
        await _databaseHelper.Database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _databaseHelper.Received(1).Update(Arg.Any<QuestionDbEntity>());
        Assert.Equal(2, _questions.Count);
        var added = _questions[1];
        Assert.NotNull(added);
        Assert.True(added.Published);
    }

    [Fact]
    public async Task ProcessMessage_Should_DeadLetterQueue_After_New_Unpublish()
    {
        _existingQuestion.Published = true;

        var subject = "ContentManagement.Entry.unpublish";

        var result =
            await _webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusErrorResult>(result);
        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cacheHandler.Received(0).RequestCacheClear(Arg.Any<CancellationToken>());
        _databaseHelper.Received(0).Update(Arg.Any<ContentComponentDbEntity>());
        _databaseHelper.Received(0).Add(Arg.Any<ContentComponentDbEntity>());
    }

    [Fact]
    public async Task ProcessMessage_Should_CompleteSuccessfully_After_Existing_Unpublish()
    {
        _questions.Add(_newQuestion);

        _newQuestion.Published = true;

        var subject = "ContentManagement.Entry.unpublish";

        var result =
            await _webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);
        Assert.IsType<ServiceBusSuccessResult>(result);
        _databaseHelper.Received(1).ClearTracking();
        await _cacheHandler.Received(1).RequestCacheClear(Arg.Any<CancellationToken>());
        _databaseHelper.Received(0).Update(Arg.Any<ContentComponentDbEntity>());
        _databaseHelper.Received(0).Add(Arg.Any<ContentComponentDbEntity>());
        await _databaseHelper.Database.ReceivedWithAnyArgs(1)
            .SetComponentPublishedAndDeletedStatuses(Arg.Any<QuestionDbEntity>(), false, false,
                Arg.Any<CancellationToken>());

        Assert.Equal(2, _questions.Count);
        var question = _questions[1];
        Assert.False(question.Published);
    }

    [Fact]
    public async Task ProcessMessage_Should_CompleteSuccessfully_After_Delete()
    {
        _questions.Add(_newQuestion);
        _newQuestion.Deleted = false;

        var subject = "ContentManagement.Entry.delete";

        var result =
            await _webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusSuccessResult>(result);
        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.ReceivedWithAnyArgs(1)
            .SetComponentPublishedAndDeletedStatuses(Arg.Any<QuestionDbEntity>(), false, true,
                Arg.Any<CancellationToken>());
        await _cacheHandler.Received(1).RequestCacheClear(Arg.Any<CancellationToken>());

        Assert.Equal(2, _questions.Count);
        Assert.True(_questions[1].Deleted);
    }

    [Theory]
    [InlineData("ContentManagement.Entry.save")]
    [InlineData("ContentManagement.Entry.auto_save")]
    public async Task
        ProcessMessage_Should_ExitEarly_When_Event_Is_Save_And_Entity_Is_Published_And_UsePreview_Is_False(
            string subject)
    {
        _newQuestion.Published = true;
        _questions.Add(_newQuestion);

        var webhookCommand = CreateWebhookToDbCommand(false);
        var result =
            await webhookCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusSuccessResult>(result);

        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cacheHandler.Received(0).RequestCacheClear(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("ContentManagement.Entry.save")]
    [InlineData("ContentManagement.Entry.auto_save")]
    public async Task ProcessMessage_Should_Save_When_Event_Is_Save_And_Entity_Is_Unpublished_And_UsePreview_Is_False(
        string subject)
    {
        _questions.Add(_newQuestion);
        _newQuestion.Published = false;

        var webhookToDbCommand = CreateWebhookToDbCommand(false);

        var result =
            await webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusSuccessResult>(result);

        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _databaseHelper.Received(1).Update(Arg.Any<QuestionDbEntity>());
        await _cacheHandler.Received(1).RequestCacheClear(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessMessage_Should_DeadLetterQueue_If_ActionIsInvalid()
    {
        var subject = "ContentManagement.Entry.INVALID";
        var result =
            await _webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusErrorResult>(result);
        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        _databaseHelper.Received(0).Update(Arg.Any<QuestionDbEntity>());
        await _cacheHandler.Received(0).RequestCacheClear(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("ContentManagement.Entry.unpublish")]
    [InlineData("ContentManagement.Entry.delete")]
    public async Task ProcessMessage_Should_DeadLetterQueue_If_Delete_Or_Unpublish_Missing_Entity(string subject)
    {
        var result =
            await _webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusErrorResult>(result);
        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        _databaseHelper.Received(0).Update(Arg.Any<QuestionDbEntity>());
        await _cacheHandler.Received(0).RequestCacheClear(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessMessage_Should_ExitEarly_When_CmsEvent_Is_Create()
    {
        var subject = "ContentManagement.Entry.create";

        var result =
            await _webhookToDbCommand.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusSuccessResult>(result);
        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        _databaseHelper.Received(0).Update(Arg.Any<QuestionDbEntity>());
        await _cacheHandler.Received(0).RequestCacheClear(Arg.Any<CancellationToken>());

        Assert.Single(_questions);
    }

    [Fact]
    public async Task ProcessMessage_Should_Save_With_Defaults_When_Required_Content_Missing()
    {
        string bodyWithMissingFields =
            "{\"metadata\":{\"tags\":[]},\"fields\":{},\"sys\":{\"type\":\"Entry\",\"id\":\"2VSR0emw0SPy8dlR9XlgfF\",\"space\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"Space\",\"id\":\"py5afvqdlxgo\"}},\"environment\":{\"sys\":{\"id\":\"dev\",\"type\":\"Link\",\"linkType\":\"Environment\"}},\"contentType\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"ContentType\",\"id\":\"question\"}},\"createdBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"5yhMQOCN9P2vGpfjyZKiey\"}},\"updatedBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"4hiJvkyVWdhTt6c4ZoDkMf\"}},\"revision\":13,\"createdAt\":\"2023-12-04T14:36:46.614Z\",\"updatedAt\":\"2023-12-15T16:16:45.034Z\"}}";

        var subject = "ContentManagement.Entry.save";

        var result =
            await _webhookToDbCommand.ProcessMessage(subject, bodyWithMissingFields, "message id",
                CancellationToken.None);

        Assert.IsType<ServiceBusSuccessResult>(result);
        _databaseHelper.Received(1).ClearTracking();
        await _databaseHelper.Database.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _databaseHelper.Received(1).Add(Arg.Any<QuestionDbEntity>());
        await _cacheHandler.Received(1).RequestCacheClear(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessEntityRemovalEvent_Should_ThrowException_When_ExistingEntity_IsNull()
    {
        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = new QuestionDbEntity(),
            CmsEvent = CmsEvent.SAVE
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _webhookToDbCommand.InvokeNonPublicAsyncMethod("ProcessEntityRemovalEvent",
                new object?[] { mappedEntity, CancellationToken.None }));
    }

    [Fact]
    public async Task UpsertEntity_Should_ThrowException_When_InvalidMappedEntity()
    {
        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = new QuestionDbEntity(),
            CmsEvent = CmsEvent.SAVE
        };

        await Assert.ThrowsAsync<NullReferenceException>(() =>
            _webhookToDbCommand.InvokeNonPublicAsyncMethod("UpsertEntity",
                new object?[] { mappedEntity }));
    }

        [Fact]
        public async Task DbSaveChanges_Should_LogMessage_When_RowsChanged()
        {
            var changedRows = 10;
            _databaseHelper.Database.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(changedRows);
            await _webhookToDbCommand.InvokeNonPublicAsyncMethod("DbSaveChanges", new object?[] { CancellationToken.None });

            var matchingLogMessages = _logger.GetMatchingReceivedMessages($"Updated {changedRows} rows in the database", LogLevel.Information);
        }

}
