using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Queues.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.UnitTests;

public class QueueWriterTests
{
    private readonly IAzureClientFactory<ServiceBusSender> _serviceBusFactory = Substitute.For<IAzureClientFactory<ServiceBusSender>>();
    private readonly ServiceBusSender _serviceBusSender = Substitute.For<ServiceBusSender>();
    private readonly ILogger<QueueWriter> _logger = Substitute.For<ILogger<QueueWriter>>();
    private readonly IQueueWriter _queueWriter;

    public QueueWriterTests()
    {
        _serviceBusFactory.CreateClient(Arg.Any<string>()).Returns((callInfo) => _serviceBusSender);
        _queueWriter = new QueueWriter(_serviceBusFactory, _logger);
    }

    [Fact]
    public async Task WriteMessage_Should_Call_ServiceBusSender()
    {
        var body = "test-body";
        var subject = "test-subject";

        await _queueWriter.WriteMessage(body, subject);

        await _serviceBusSender.Received(1)
                        .SendMessageAsync(Arg.Any<ServiceBusMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WriteMessage_Should_Handle_Exception()
    {
        var body = "test-body";
        var subject = "test-subject";

        var exception = new Exception("Thrown exception message");

        _serviceBusSender.SendMessageAsync(Arg.Any<ServiceBusMessage>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var result = await _queueWriter.WriteMessage(body, subject);

        Assert.False(result.Success);
        Assert.Equal(exception.Message, result.ErrorMessage);

        await _serviceBusSender.Received(1)
            .SendMessageAsync(Arg.Any<ServiceBusMessage>(), Arg.Any<CancellationToken>());

        var loggedMessages = _logger.ReceivedLogMessages().ToArray();

        Assert.Single(loggedMessages);
        Assert.Contains("Error sending service bus message.", loggedMessages[0].Message);
    }
}
