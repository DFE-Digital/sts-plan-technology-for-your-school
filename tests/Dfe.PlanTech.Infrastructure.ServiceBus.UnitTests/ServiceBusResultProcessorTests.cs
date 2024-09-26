using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Infrastructure.ServiceBus.Results;
using Dfe.PlanTech.Infrastructure.ServiceBus.Retry;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.UnitTests;

public class ServiceBusResultProcessorTests
{
    private readonly IMessageRetryHandler _retryHandler = Substitute.For<IMessageRetryHandler>();
    private readonly ILogger<ServiceBusResultProcessor> _logger = Substitute.For<ILogger<ServiceBusResultProcessor>>();
    private readonly ServiceBusReceivedMessage _message = Substitute.For<ServiceBusReceivedMessage>();
    private readonly ServiceBusReceiver _receiver = Substitute.For<ServiceBusReceiver>();

    private readonly ServiceBusResultProcessor _processor;
    private readonly ProcessMessageEventArgs _eventArgs;

    public ServiceBusResultProcessorTests()
    {
        _eventArgs = Substitute.For<ProcessMessageEventArgs>(_message, _receiver, CancellationToken.None);
        _processor = new ServiceBusResultProcessor(_retryHandler, _logger);
    }

    [Fact]
    public async Task Should_Complete_Successful_Message()
    {
        await _processor.ProcessMessageResult(_eventArgs, new ServiceBusSuccessResult() { }, default);

        await _eventArgs.Received(1).CompleteMessageAsync(_message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_DeadLetter_Failed_Message_If_NonRetryable()
    {
        string failureReason = "failure reason";
        string failureDescription = "failure description";

        var result = new ServiceBusErrorResult(failureReason, failureDescription, false);

        await _processor.ProcessMessageResult(_eventArgs, result, default);

        await _eventArgs.Received(1).DeadLetterMessageAsync(_message, null, result.Reason, result.Description, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Complete_Failed_Message_If_Retryable_And_ShouldRetry()
    {
        string failureReason = "failure reason";
        string failureDescription = "failure description";
        var result = new ServiceBusErrorResult(failureReason, failureDescription, true);
        _retryHandler.RetryRequired(_message, Arg.Any<CancellationToken>()).Returns(true);

        await _processor.ProcessMessageResult(_eventArgs, result, default);

        await _eventArgs.Received(1).CompleteMessageAsync(_message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Complete_DeadLetter_Message_If_Retryable_But_ExceededRetries()
    {
        string failureReason = "failure reason";
        string failureDescription = "failure description";
        var result = new ServiceBusErrorResult(failureReason, failureDescription, true);
        _retryHandler.RetryRequired(_message, Arg.Any<CancellationToken>()).Returns(false);

        await _processor.ProcessMessageResult(_eventArgs, result, default);

        await _eventArgs.Received(1).DeadLetterMessageAsync(_message, null, result.Reason, result.Description, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_LogError_When_Unknown_ServiceBusResultType()
    {
        var result = new MockServiceBusResult();
        await _processor.ProcessMessageResult(_eventArgs, result, default);

        var expectedLogMessage = $"Unexpected service bus result type: {result.GetType().Name}";
        var matchingLogMessages = _logger.GetMatchingReceivedMessages(expectedLogMessage, LogLevel.Error);

        Assert.Single(matchingLogMessages);
    }
}

public class MockServiceBusResult : IServiceBusResult { }
