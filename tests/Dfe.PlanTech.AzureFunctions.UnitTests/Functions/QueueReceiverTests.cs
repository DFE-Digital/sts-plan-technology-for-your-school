using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;
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

    private readonly static ContentComponentDbEntityImplementation _contentComponent = new() { Archived = true, Published = true, Deleted = true, Id = _contentId };
    private readonly static ContentComponentDbEntityImplementation _otherContentComponent = new() { Archived = true, Published = true, Deleted = true, Id = "other-content-component" };

    private readonly List<ContentComponentDbEntityImplementation> _existing = new()
    {
        _otherContentComponent
    };


    public QueueReceiverTests()
    {
        _loggerFactoryMock = Substitute.For<ILoggerFactory>();
        _loggerMock = Substitute.For<ILogger>();

        _loggerFactoryMock.CreateLogger<Arg.AnyType>().Returns((callinfo) =>
        {
            return _loggerMock;
        });

        _cmsDbContextMock = Substitute.For<CmsDbContext>();
        IQueryable<ContentComponentDbEntityImplementation> queryable = _existing.AsQueryable();
        _cmsDbContextMock.SaveChangesAsync().Returns(1);

        var asyncProvider = new AsyncQueryProvider<ContentComponentDbEntityImplementation>(queryable.Provider);

        var mockSet = Substitute.For<DbSet<ContentComponentDbEntityImplementation>, IQueryable<ContentComponentDbEntityImplementation>>();
        ((IQueryable<ContentComponentDbEntityImplementation>)mockSet).Provider.Returns(asyncProvider);
        ((IQueryable<ContentComponentDbEntityImplementation>)mockSet).Expression.Returns(queryable.Expression);
        ((IQueryable<ContentComponentDbEntityImplementation>)mockSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<ContentComponentDbEntityImplementation>)mockSet).GetEnumerator().Returns(queryable.GetEnumerator());

        var entityTypeMock = Substitute.For<IEntityType>();
        entityTypeMock.ClrType.Returns(typeof(ContentComponentDbEntityImplementation));

        _cmsDbContextMock.Model.FindEntityType(Arg.Any<Type>()).Returns(callInfo =>
        {
            return entityTypeMock;
        });

        _cmsDbContextMock.Set<ContentComponentDbEntityImplementation>().Returns(mockSet);

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        Type questionDbEntityType = typeof(QuestionDbEntity);

        _jsonToEntityMappers = Substitute.For<JsonToEntityMappers>(new JsonToDbMapper[] { new JsonToDbMapperImplementation(questionDbEntityType, _loggerMock, jsonOptions) }, jsonOptions);

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
        _contentComponent.Archived = true;

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.unarchive";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        var added = _addedObject as ContentComponentDbEntity;
        Assert.NotNull(added);
        Assert.False(added.Archived);
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

        await _queueReceiver.QueueReceiverDbWriter(new ServiceBusReceivedMessage[] { serviceBusReceivedMessage }, serviceBusMessageActionsMock, CancellationToken.None);

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

        await _queueReceiver.QueueReceiverDbWriter(new ServiceBusReceivedMessage[] { serviceBusReceivedMessage }, serviceBusMessageActionsMock, CancellationToken.None);

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

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.delete";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter(new ServiceBusReceivedMessage[] { serviceBusReceivedMessage }, serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        var added = _addedObject as ContentComponentDbEntity;
        Assert.NotNull(added);
        Assert.True(added.Deleted);
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
}