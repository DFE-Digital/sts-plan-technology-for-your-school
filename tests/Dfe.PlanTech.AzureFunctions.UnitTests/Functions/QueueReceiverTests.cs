using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class QueueReceiverTests
{
    private const string bodyJsonStr = "{\"metadata\":{\"tags\":[]},\"fields\":{\"internalName\":{\"en-US\":\"TestingQuestion\"},\"text\":{\"en-US\":\"TestingQuestion\"},\"helpText\":{\"en-US\":\"HelpText\"},\"answers\":{\"en-US\":[{\"sys\":{\"type\":\"Link\",\"linkType\":\"Entry\",\"id\":\"4QscetbCYG4MUsGdoDU0C3\"}}]},\"slug\":{\"en-US\":\"testing-slug\"}},\"sys\":{\"type\":\"Entry\",\"id\":\"2VSR0emw0SPy8dlR9XlgfF\",\"space\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"Space\",\"id\":\"py5afvqdlxgo\"}},\"environment\":{\"sys\":{\"id\":\"dev\",\"type\":\"Link\",\"linkType\":\"Environment\"}},\"contentType\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"ContentType\",\"id\":\"question\"}},\"createdBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"5yhMQOCN9P2vGpfjyZKiey\"}},\"updatedBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"4hiJvkyVWdhTt6c4ZoDkMf\"}},\"revision\":13,\"createdAt\":\"2023-12-04T14:36:46.614Z\",\"updatedAt\":\"2023-12-15T16:16:45.034Z\"}}";
    private const string _contentId = "2VSR0emw0SPy8dlR9XlgfF";
    private readonly QueueReceiver _queueReceiver;

    private readonly ILoggerFactory _loggerFactoryMock;
    private readonly ILogger _loggerMock;
    private readonly CmsDbContext _cmsDbContextMock;
    private readonly JsonToEntityMappers _jsonToEntityMappers;

    private object? _addedObject = null;

    private readonly static QuestionDbEntity _contentComponent = new() { Archived = false, Published = true, Deleted = false, Id = _contentId };
    private readonly static QuestionDbEntity _otherContentComponent = new() { Archived = false, Published = true, Deleted = false, Id = "other-content-component" };

    private readonly List<QuestionDbEntity> _existing = [_otherContentComponent];


    public QueueReceiverTests()
    {
        _loggerFactoryMock = Substitute.For<ILoggerFactory>();
        _loggerMock = Substitute.For<ILogger>();

        _loggerFactoryMock.CreateLogger<Arg.AnyType>().Returns((callinfo) =>
        {
            return _loggerMock;
        });

        _cmsDbContextMock = Substitute.For<CmsDbContext>();
        IQueryable<QuestionDbEntity> queryable = _existing.AsQueryable();
        _cmsDbContextMock.SaveChangesAsync().Returns(1);

        var asyncProvider = new AsyncQueryProvider<QuestionDbEntity>(queryable.Provider);

        var mockSet = Substitute.For<DbSet<QuestionDbEntity>, IQueryable<QuestionDbEntity>>();
        ((IQueryable<QuestionDbEntity>)mockSet).Provider.Returns(asyncProvider);
        ((IQueryable<QuestionDbEntity>)mockSet).Expression.Returns(queryable.Expression);
        ((IQueryable<QuestionDbEntity>)mockSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<QuestionDbEntity>)mockSet).GetEnumerator().Returns(queryable.GetEnumerator());

        var entityTypeMock = Substitute.For<IEntityType>();
        entityTypeMock.ClrType.Returns(typeof(QuestionDbEntity));

        _cmsDbContextMock.Model.FindEntityType(typeof(QuestionDbEntity)).Returns(entityTypeMock);

        HashSet<string> nonNullablePropertyNames = ["Text", "Slug", "Answers"];

        IEnumerable<IProperty>? questionProperties = typeof(QuestionDbEntity).GetProperties().Select(property =>
        {
            var propertySub = Substitute.For<IProperty>();
            propertySub.PropertyInfo.Returns(property);
            propertySub.IsNullable.Returns(!nonNullablePropertyNames.Contains(property.Name));

            return propertySub;
        });

        entityTypeMock.GetProperties().Returns(questionProperties);

        _cmsDbContextMock.Set<QuestionDbEntity>().Returns(mockSet);

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        Type questionDbEntityType = typeof(QuestionDbEntity);

        DbSet<AnswerDbEntity> _answerDbSet = Substitute.For<DbSet<AnswerDbEntity>>();

        _cmsDbContextMock.Answers = _answerDbSet;

        _answerDbSet.WhenForAnyArgs(answerDbSet => answerDbSet.Attach(Arg.Any<AnswerDbEntity>()))
                    .Do(callinfo =>
                    {
                        var answer = callinfo.ArgAt<AnswerDbEntity>(0);
                    });

        _jsonToEntityMappers = Substitute.For<JsonToEntityMappers>(new JsonToDbMapper[] { new QuestionMapper(new EntityRetriever(_cmsDbContextMock), new EntityUpdater(Substitute.For<ILogger<EntityUpdater>>(), _cmsDbContextMock), _cmsDbContextMock, Substitute.For<ILogger<QuestionMapper>>(), jsonOptions) }, jsonOptions);

        _queueReceiver = new QueueReceiver(new ContentfulOptions(true), _loggerFactoryMock, _cmsDbContextMock, _jsonToEntityMappers);

        _cmsDbContextMock.Add(Arg.Any<ContentComponentDbEntity>()).Returns(callinfo =>
        {
            var added = callinfo.ArgAt<ContentComponentDbEntity>(0);
            _addedObject = added;

            return null!;
        });
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_Execute_Successfully()
    {
        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.save";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>());
        await _cmsDbContextMock.ReceivedWithAnyArgs(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _cmsDbContextMock.ReceivedWithAnyArgs(1).Add(Arg.Any<ContentComponentDbEntity>());

        Assert.NotNull(_addedObject);

        var asContentComponentDbEntity = _addedObject as ContentComponentDbEntity;

        Assert.NotNull(asContentComponentDbEntity);

        Assert.Equal(_contentId, asContentComponentDbEntity.Id);
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_DeadLetter_Failed_Operation()
    {
        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        string nonMappableJson = "\"INVALID\":\"CONTENT\"";

        var subject = "ContentManagement.Entry.save";
        var serviceBusMessage = new ServiceBusMessage(nonMappableJson) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().DeadLetterMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_MapExistingDbEntity_To_Message()
    {
        _existing.Add(_contentComponent);
        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.save";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());
        _cmsDbContextMock.ReceivedWithAnyArgs(0).Add(Arg.Any<ContentComponentDbEntity>());
        await _cmsDbContextMock.ReceivedWithAnyArgs(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueRecieverDbWriter_Should_CompleteSuccessfully_After_Archive()
    {
        _contentComponent.Archived = false;

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.archive";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        var added = _addedObject as ContentComponentDbEntity;
        Assert.NotNull(added);
        Assert.True(added.Archived);
    }

    [Fact]
    public async Task QueueRecieverDbWriter_Should_CompleteSuccessfully_After_Unarchive()
    {
        _existing.Add(_contentComponent);
        _contentComponent.Archived = true;

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.unarchive";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        Assert.Null(_addedObject);
    }

    [Fact]
    public async Task QueueRecieverDbWriter_Should_CompleteSuccessfully_After_Publish()
    {
        _contentComponent.Published = true;

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.publish";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        var added = _addedObject as ContentComponentDbEntity;
        Assert.NotNull(added);
        Assert.True(added.Published);
    }

    [Fact]
    public async Task QueueRecieverDbWriter_Should_DeadLetterQueue_After_New_Unpublish()
    {
        _contentComponent.Published = false;

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.unpublish";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().DeadLetterMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueRecieverDbWriter_Should_CompleteSuccessfully_After_Existing_Unpublish()
    {
        _existing.Add(_contentComponent);

        _contentComponent.Published = false;

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.unpublish";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());
        _cmsDbContextMock.ReceivedWithAnyArgs(0).Add(Arg.Any<ContentComponentDbEntity>());
        await _cmsDbContextMock.ReceivedWithAnyArgs(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueRecieverDbWriter_Should_CompleteSuccessfully_After_Delete()
    {
        _contentComponent.Deleted = false;
        _existing.Add(_contentComponent);

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.delete";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received()
                                          .CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        Assert.Null(_addedObject);
    }

    [Fact]
    public async Task QueueRecieverDbWriter_Should_ExitEarly_When_Event_Is_Save_And_UsePreview_Is_False()
    {
        var queueReceiver = new QueueReceiver(new ContentfulOptions(false), _loggerFactoryMock, _cmsDbContextMock, _jsonToEntityMappers);

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.save";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        var added = _addedObject as ContentComponentDbEntity;
        Assert.Null(added);
    }


    [Fact]
    public async Task QueueRecieverDbWriter_Should_ExitEarly_When_Event_Is_AutoSave_And_UsePreview_Is_False()
    {
        var queueReceiver = new QueueReceiver(new ContentfulOptions(false), _loggerFactoryMock, _cmsDbContextMock, _jsonToEntityMappers);

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.auto_save";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received()
                                          .CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        var added = _addedObject as ContentComponentDbEntity;
        Assert.Null(added);
    }

    [Fact]
    public async Task QueueRecieverDbWriter_Should_DeadLetterQueue_If_ActionIsInvalid()
    {
        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.INVALID";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().DeadLetterMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueRecieverDbWriter_Should_ExitEarly_When_Component_IsInvalid()
    {
        string invalidBodyJsonStr = "{\"metadata\":{\"tags\":[]},\"sys\":{\"space\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"Space\",\"id\":\"py5afvqdlxgo\"}},\"id\":\"6G1UpN2x7vrtoSZdpg2nW7\",\"type\":\"Entry\",\"createdAt\":\"2024-02-12T11:40:25.426Z\",\"updatedAt\":\"2024-02-12T11:40:38.648Z\",\"environment\":{\"sys\":{\"id\":\"dev\",\"type\":\"Link\",\"linkType\":\"Environment\"}},\"createdBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"4hiJvkyVWdhTt6c4ZoDkMf\"}},\"updatedBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"4hiJvkyVWdhTt6c4ZoDkMf\"}},\"publishedCounter\":0,\"version\":2,\"automationTags\":[],\"contentType\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"ContentType\",\"id\":\"question\"}}},\"fields\":{\"internalName\":{\"en-US\":\"TestInternalName\"},\"slug\":{\"en-US\":\"test-slug\"}}}";

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.auto_save";
        var serviceBusMessage = new ServiceBusMessage(invalidBodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received()
                                          .CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        var added = _addedObject as ContentComponentDbEntity;
        Assert.Null(added);
    }

    [Fact]
    public async Task QueueRecieverDbWriter_Should_ExitEarly_When_CmsEvent_Is_Create()
    {
        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.create";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received()
                                          .CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        var added = _addedObject as ContentComponentDbEntity;
        Assert.Null(added);
    }
}