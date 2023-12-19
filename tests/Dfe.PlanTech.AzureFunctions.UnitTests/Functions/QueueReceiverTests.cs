using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class QueueReceiverTests
{
    private readonly QueueReceiver _queueReceiver;

    private readonly ILoggerFactory _loggerFactoryMock;
    private readonly ILogger _loggerMock;
    private readonly CmsDbContext _cmsDbContextMock;
    private readonly JsonToEntityMappers _jsonToEntityMappers;

    public QueueReceiverTests()
    {
        _loggerFactoryMock = Substitute.For<ILoggerFactory>();
        _loggerMock = Substitute.For<ILogger>();

        _loggerFactoryMock.CreateLogger<Arg.AnyType>().Returns((callinfo) =>
        {
            return _loggerMock;
        });

        _cmsDbContextMock = Substitute.For<CmsDbContext>();

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        Type buttonWithLinkDbEntityType = typeof(ButtonWithLinkDbEntity);

        //_jsonToEntityMappers = new JsonToEntityMappers(new JsonToDbMapper[] { new JsonToDbMapperImplementation(buttonWithLinkDbEntityType, _loggerMock, jsonOptions) }, jsonOptions);

        //JsonToDbMapperImplementation jsonToDbMapperImplementation = Substitute.For<JsonToDbMapperImplementation>(buttonWithLinkDbEntityType, _loggerMock, jsonOptions);

        _jsonToEntityMappers = Substitute.For<JsonToEntityMappers>(new JsonToDbMapper[] { new JsonToDbMapperImplementation(buttonWithLinkDbEntityType, _loggerMock, jsonOptions) }, jsonOptions);

        _queueReceiver = new QueueReceiver(_loggerFactoryMock, _cmsDbContextMock, _jsonToEntityMappers);
    }

    [Fact]
    public async Task QueueReceiverDbWriter_Should_Execute_Successfully()
    {
        ServiceBusReceivedMessage serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();
        ServiceBusMessageActions serviceBusMessageActionsMock = Substitute.For<ServiceBusMessageActions>();

        var subject = "ContentManagement.Entry.create";
        var body = "{\"Body\":\"RequestBody\"}";
        var serviceBusMessage = new ServiceBusMessage(body) { Subject = subject };

        ContentComponentDbEntity contentComponentDbEntity = new ButtonWithLinkDbEntity();

        //_jsonToEntityMappers.ToEntity(body).Returns(contentComponentDbEntity);

        ServiceBusReceivedMessage serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        await _queueReceiver.QueueReceiverDbWriter(new ServiceBusReceivedMessage[] { serviceBusReceivedMessage }, serviceBusMessageActionsMock);
    }
}