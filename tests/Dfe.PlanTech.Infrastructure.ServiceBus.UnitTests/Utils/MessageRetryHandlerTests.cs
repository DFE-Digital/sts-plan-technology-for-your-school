using System.Text;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Infrastructure.ServiceBus.Retry;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.UnitTests.Utils;

public class MessageRetryHandlerTests
{

    private const string bodyJsonStr = "{\"metadata\":{\"tags\":[]},\"fields\":{\"internalName\":{\"en-US\":\"TestingQuestion\"},\"text\":{\"en-US\":\"TestingQuestion\"},\"helpText\":{\"en-US\":\"HelpText\"},\"answers\":{\"en-US\":[{\"sys\":{\"type\":\"Link\",\"linkType\":\"Entry\",\"id\":\"4QscetbCYG4MUsGdoDU0C3\"}}]},\"slug\":{\"en-US\":\"testing-slug\"}},\"sys\":{\"type\":\"Entry\",\"id\":\"2VSR0emw0SPy8dlR9XlgfF\",\"space\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"Space\",\"id\":\"py5afvqdlxgo\"}},\"environment\":{\"sys\":{\"id\":\"dev\",\"type\":\"Link\",\"linkType\":\"Environment\"}},\"contentType\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"ContentType\",\"id\":\"question\"}},\"createdBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"5yhMQOCN9P2vGpfjyZKiey\"}},\"updatedBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"4hiJvkyVWdhTt6c4ZoDkMf\"}},\"revision\":13,\"createdAt\":\"2023-12-04T14:36:46.614Z\",\"updatedAt\":\"2023-12-15T16:16:45.034Z\"}}";

    private readonly IAzureClientFactory<ServiceBusSender> _serviceBusFactory = Substitute.For<IAzureClientFactory<ServiceBusSender>>();
    private readonly ServiceBusSender _serviceBusSender = Substitute.For<ServiceBusSender>();
    private readonly MessageRetryHandler _messageRetryHandler;
    private readonly IOptions<MessageRetryHandlingOptions> _options = Options.Create(new MessageRetryHandlingOptions());

    public MessageRetryHandlerTests()
    {
        _serviceBusFactory.CreateClient(Arg.Any<string>()).Returns((callInfo) => _serviceBusSender);
        _messageRetryHandler = new MessageRetryHandler(_serviceBusFactory, _options);
    }


    [Fact]
    public async Task MessageRetryHandler_Should_Retry_Message_If_MaxRetries_Are_Not_Exhausted()
    {
        var serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();

        var subject = "ContentManagement.Entry.publish";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        var serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        var retryRequired = await _messageRetryHandler.RetryRequired(serviceBusReceivedMessage, CancellationToken.None);

        await _serviceBusSender.Received(1).SendMessageAsync(Arg.Any<ServiceBusMessage>());

        Assert.True(retryRequired);
    }

    [Fact]
    public async Task MessageRetryHandler_Should_Not_Retry_Message_If_MaxRetries_Are_Exhausted()
    {
        var serviceBusReceivedMessageMock = Substitute.For<ServiceBusReceivedMessage>();

        var subject = "ContentManagement.Entry.publish";
        var serviceBusMessage = new ServiceBusMessage(bodyJsonStr) { Subject = subject };

        serviceBusMessage.ApplicationProperties.Add("DeliveryAttempts", 4);

        var serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(serviceBusMessage.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(serviceBusReceivedMessageMock.LockToken)));

        var retryRequired = await _messageRetryHandler.RetryRequired(serviceBusReceivedMessage, CancellationToken.None);

        Assert.False(retryRequired);
    }
}
