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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dfe.PlanTech.AzureFunctions.Services;
using Dfe.PlanTech.AzureFunctions.Utils;
using NSubstitute.ExceptionExtensions;
using MockQueryable.NSubstitute;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NSubstitute.ReceivedExtensions;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class QueueReceiverTests
{
    private const string bodyJsonStr = "{\"metadata\":{\"tags\":[]},\"fields\":{\"internalName\":{\"en-US\":\"TestingQuestion\"},\"text\":{\"en-US\":\"TestingQuestion\"},\"helpText\":{\"en-US\":\"HelpText\"},\"answers\":{\"en-US\":[{\"sys\":{\"type\":\"Link\",\"linkType\":\"Entry\",\"id\":\"4QscetbCYG4MUsGdoDU0C3\"}}]},\"slug\":{\"en-US\":\"testing-slug\"}},\"sys\":{\"type\":\"Entry\",\"id\":\"2VSR0emw0SPy8dlR9XlgfF\",\"space\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"Space\",\"id\":\"py5afvqdlxgo\"}},\"environment\":{\"sys\":{\"id\":\"dev\",\"type\":\"Link\",\"linkType\":\"Environment\"}},\"contentType\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"ContentType\",\"id\":\"question\"}},\"createdBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"5yhMQOCN9P2vGpfjyZKiey\"}},\"updatedBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"4hiJvkyVWdhTt6c4ZoDkMf\"}},\"revision\":13,\"createdAt\":\"2023-12-04T14:36:46.614Z\",\"updatedAt\":\"2023-12-15T16:16:45.034Z\"}}";
    private const string _contentId = "2VSR0emw0SPy8dlR9XlgfF";

    private readonly QueueReceiver _queueReceiver;

    private readonly ILoggerFactory _loggerFactoryMock;
    private readonly ILogger _loggerMock;
    private readonly CmsDbContext _cmsDbContextMock;
    private readonly EntityRetriever _entityRetrieverMock;
    private readonly JsonToEntityMappers _jsonToEntityMappers;
    private readonly IMessageRetryHandler _messageRetryHandlerMock;
    private readonly ICacheHandler _cacheHandler;


    private readonly static QuestionDbEntity _contentComponent = new() { Archived = false, Published = true, Deleted = false, Id = _contentId };
    private readonly static QuestionDbEntity _otherContentComponent = new() { Archived = false, Published = true, Deleted = false, Id = "other-content-component" };

    private readonly List<QuestionDbEntity> _questions = [_otherContentComponent];
    private readonly List<AnswerDbEntity> _answers = [];

    private readonly List<ContentComponentDbEntity> _contentComponents = [];

    public QueueReceiverTests()
    {
        _loggerFactoryMock = Substitute.For<ILoggerFactory>();
        _loggerMock = Substitute.For<ILogger>();
        _messageRetryHandlerMock = Substitute.For<IMessageRetryHandler>();
        _cacheHandler = Substitute.For<ICacheHandler>();

        _loggerFactoryMock.CreateLogger<Arg.AnyType>().Returns((callinfo) =>
        {
            return _loggerMock;
        });

        _cmsDbContextMock = Substitute.For<CmsDbContext>();
        _entityRetrieverMock = Substitute.For<EntityRetriever>(_cmsDbContextMock);

        _cmsDbContextMock.SaveChangesAsync().Returns(1);

        var mockQuestionSet = _questions.AsQueryable().BuildMockDbSet();
        _cmsDbContextMock.Questions = mockQuestionSet;
        _cmsDbContextMock.Set<QuestionDbEntity>().Returns(mockQuestionSet);

        _cmsDbContextMock.When(db => db.Add(Arg.Any<ContentComponentDbEntity>()))
                                    .Do((callinfo) =>
                                    {
                                        var contentComponent = callinfo.ArgAt<ContentComponentDbEntity>(0);

                                        if (contentComponent is QuestionDbEntity question)
                                        {
                                            _questions.Add(question);
                                        }
                                        else
                                        {
                                            Console.Write("Shouldn't hit this");
                                        }
                                    });

        var answersDbSet = _answers.AsQueryable().BuildMockDbSet();
        _cmsDbContextMock.Answers = answersDbSet;

        MockEntityType();

        _jsonToEntityMappers = CreateMappers();
        _cmsDbContextMock.SetComponentPublishedAndDeletedStatuses(Arg.Any<ContentComponentDbEntity>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((callinfo) =>
            {
                var contentComponent = callinfo.ArgAt<ContentComponentDbEntity>(0);
                var published = callinfo.ArgAt<bool>(1);
                var deleted = callinfo.ArgAt<bool>(2);

                contentComponent.Published = published;
                contentComponent.Deleted = deleted;

                return Task.FromResult(1);
            });

        _queueReceiver = new(new ContentfulOptions(true), _loggerFactoryMock, _cmsDbContextMock, _jsonToEntityMappers, _messageRetryHandlerMock, _cacheHandler);
        DbSet<ContentComponentDbEntity> contentComponentsMock = MockContentComponents();
        _cmsDbContextMock.ContentComponents = contentComponentsMock;

    }

    private DbSet<ContentComponentDbEntity> MockContentComponents()
    {
        _contentComponents.Add(_contentComponent);
        _contentComponents.Add(_otherContentComponent);

        var contentComponentsMock = _contentComponents.AsQueryable().BuildMockDbSet();
        return contentComponentsMock;
    }

    private void MockEntityType()
    {
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
    }

    private JsonToEntityMappers CreateMappers()
    {
        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        return Substitute.For<JsonToEntityMappers>(new JsonToDbMapper[] { new QuestionMapper(new EntityRetriever(_cmsDbContextMock), new EntityUpdater(Substitute.For<ILogger<EntityUpdater>>(), _cmsDbContextMock), _cmsDbContextMock, Substitute.For<ILogger<QuestionMapper>>(), jsonOptions) }, jsonOptions);
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


        Assert.Equal(2, _questions.Count);

        var addedQuestion = _questions[1];

        var asContentComponentDbEntity = addedQuestion as ContentComponentDbEntity;

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
        _questions.Add(_contentComponent);
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
    public async Task QueueReceiverDbWriter_Should_CompleteSuccessfully_After_Archive()
    {
        _contentComponent.Archived = false;

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.archive";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        Assert.Equal(2, _questions.Count);
        var added = _questions[1];
        Assert.NotNull(added);
        Assert.True(added.Archived);
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_CompleteSuccessfully_After_Unarchive()
    {
        _questions.Add(_contentComponent);
        _contentComponent.Archived = true;

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.unarchive";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        Assert.Equal(2, _questions.Count);
        var added = _questions[1];
        Assert.NotNull(added);
        Assert.False(added.Archived);
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_CompleteSuccessfully_After_Publish()
    {
        _contentComponent.Published = true;

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.publish";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        Assert.Equal(2, _questions.Count);
        var added = _questions[1];
        Assert.NotNull(added);
        Assert.True(added.Published);
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_DeadLetterQueue_After_New_Unpublish()
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
    public async Task QueueReceiverDbWriter_Should_CompleteSuccessfully_After_Existing_Unpublish()
    {
        _questions.Add(_contentComponent);

        _contentComponent.Published = true;

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.unpublish";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());
        _cmsDbContextMock.ReceivedWithAnyArgs(0).Add(Arg.Any<ContentComponentDbEntity>());
        await _cmsDbContextMock.ReceivedWithAnyArgs(0).SaveChangesAsync(Arg.Any<CancellationToken>());

        await _cmsDbContextMock.ReceivedWithAnyArgs(1).SetComponentPublishedAndDeletedStatuses(Arg.Any<QuestionDbEntity>(), false, false, Arg.Any<CancellationToken>());
        Assert.Equal(2, _questions.Count);

        var question = _questions[1];
        Assert.False(question.Published);
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_CompleteSuccessfully_After_Delete()
    {
        _contentComponent.Deleted = false;
        _questions.Add(_contentComponent);

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.delete";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received()
                                          .CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        Assert.Equal(2, _questions.Count);
        Assert.True(_questions[1].Deleted);
    }

    [Theory]
    [InlineData("ContentManagement.Entry.save")]
    [InlineData("ContentManagement.Entry.auto_save")]
    public async Task QueueReceiverDbWriter_Should_ExitEarly_When_Event_Is_Save_And_Entity_Is_Published_And_UsePreview_Is_False(string subject)
    {
        _contentComponent.Published = true;
        _questions.Add(_contentComponent);
        _entityRetrieverMock
            .When(mock => mock
                .GetExistingDbEntity(Arg.Is<ContentComponentDbEntity>(entity => entity.Id == _contentId), default)
                .Returns(_contentComponent));
        var queueReceiver = new QueueReceiver(new ContentfulOptions(false), _loggerFactoryMock, _cmsDbContextMock, _jsonToEntityMappers, _messageRetryHandlerMock, _cacheHandler);

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        await _cmsDbContextMock.Received(0).SaveChangesAsync();
        await _cacheHandler.Received(0).RequestCacheClear(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("ContentManagement.Entry.save")]
    [InlineData("ContentManagement.Entry.auto_save")]
    public async Task QueueReceiverDbWriter_Should_Save_When_Event_Is_Save_And_Entity_Is_Unpublished_And_UsePreview_Is_False(string subject)
    {
        _entityRetrieverMock
            .When(mock => mock
                .GetExistingDbEntity(Arg.Is<ContentComponentDbEntity>(entity => entity.Id == _otherContentComponent.Id), default)
                .Returns(_otherContentComponent));
        var queueReceiver = new QueueReceiver(new ContentfulOptions(false), _loggerFactoryMock, _cmsDbContextMock, _jsonToEntityMappers, _messageRetryHandlerMock, _cacheHandler);

        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        await _cmsDbContextMock.Received(1).SaveChangesAsync();
        await _cacheHandler.Received(1).RequestCacheClear(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_DeadLetterQueue_If_ActionIsInvalid()
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
    public async Task QueueReceiverDbWriter_Should_ExitEarly_When_Component_IsInvalid()
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

        Assert.Single(_questions);
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_ExitEarly_When_CmsEvent_Is_Create()
    {
        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.create";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received()
                                          .CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());

        Assert.Single(_questions);
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_Redeliver_A_Message_If_There_Is_A_Transient_Exception()
    {
        var serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        var serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.publish";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        _cmsDbContextMock.SaveChangesAsync().ThrowsAsync(new Exception("Something went wrong"));

        var serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        _messageRetryHandlerMock.RetryRequired(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>()).Returns(true);

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().CompleteMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_DeadLetter_A_Message_If_Max_Message_Delivery_Count_Is_Exhausted()
    {
        var serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        var serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.publish";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        serviceBusMessage.ApplicationProperties.Add("DeliveryAttempts", 4);

        _cmsDbContextMock.SaveChangesAsync().ThrowsAsync(new Exception("Something went wrong"));

        var serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter([serviceBusReceivedMessage], serviceBusMessageActionsMock, CancellationToken.None);

        await serviceBusMessageActionsMock.Received().DeadLetterMessageAsync(Arg.Any<ServiceBusReceivedMessage>(), null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessEntityRemovalEvent_Should_Throw_Exception_If_Entity_Null()
    {
        await Assert.ThrowsAnyAsync<Exception>(() => _queueReceiver.ProcessEntityRemovalEvent(null!, CancellationToken.None));
    }
}