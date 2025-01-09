using System.Text;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Infrastructure.ServiceBus.Results;
using Dfe.PlanTech.Infrastructure.ServiceBus.Retry;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.UnitTests;

public class ContentfulServiceBusProcessorTests
{
    private readonly IAzureClientFactory<ServiceBusProcessor> _processorFactory = Substitute.For<IAzureClientFactory<ServiceBusProcessor>>();
    private readonly ServiceBusProcessor _serviceBusProcessor = Substitute.For<ServiceBusProcessor>();
    private readonly ServiceBusReceiver _serviceBusReceiver = Substitute.For<ServiceBusReceiver>();
    private readonly IMessageRetryHandler _retryHandler = Substitute.For<IMessageRetryHandler>();
    private readonly ILogger<ContentfulServiceBusProcessor> _logger = Substitute.For<ILogger<ContentfulServiceBusProcessor>>();
    private readonly IWebhookToDbCommand _webhookToDbCommand = Substitute.For<IWebhookToDbCommand>();
    private readonly IServiceScopeFactory _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
    private readonly IOptions<ServiceBusOptions> _options = Substitute.For<IOptions<ServiceBusOptions>>();
    private readonly IServiceBusResultProcessor _serviceBusResultProcessor;
    private readonly ContentfulServiceBusProcessor _contentfulServiceBusProcessor;

    public ContentfulServiceBusProcessorTests()
    {
        var scope = Substitute.For<IServiceScope>();
        _serviceScopeFactory.CreateScope().Returns(scope);

        var serviceProvider = Substitute.For<IServiceProvider>();
        scope.ServiceProvider.Returns(serviceProvider);

        serviceProvider.GetService<IWebhookToDbCommand>().Returns(_webhookToDbCommand);

        _serviceBusResultProcessor = Substitute.For<IServiceBusResultProcessor>();
        _processorFactory.CreateClient("contentfulprocessor").Returns(_serviceBusProcessor);
        _options.Value.Returns(new ServiceBusOptions() { EnableQueueReading = true });
        _contentfulServiceBusProcessor = new ContentfulServiceBusProcessor(_processorFactory, _serviceBusResultProcessor, _logger, _serviceScopeFactory, _options);
    }

    [Fact]
    public async Task EnableQueueReading_Should_PreventQueueProcessing_When_False()
    {
        _options.Value.Returns(new ServiceBusOptions() { EnableQueueReading = false });
        var contentfulServiceBusProcessor = new ContentfulServiceBusProcessor(_processorFactory, _serviceBusResultProcessor, _logger, _serviceScopeFactory, _options);

        await contentfulServiceBusProcessor.InvokeNonPublicAsyncMethod("ExecuteAsync", [CancellationToken.None]);

        Assert.Empty(_serviceBusProcessor.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStartProcessingMessages()
    {
        await _contentfulServiceBusProcessor.StartAsync(CancellationToken.None);
        await _serviceBusProcessor.Received(1).StartProcessingAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StopProcessingAsync_ShouldStopAndDisposeProcessor()
    {
        await _contentfulServiceBusProcessor.InvokeNonPublicAsyncMethod("StopProcessingAsync", null);
        await _serviceBusProcessor.Received(1).StopProcessingAsync();
        await _serviceBusProcessor.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task MessageHandler_Processes_WebhookSuccess()
    {
        var body = "message body";
        var subject = "message subject";
        var id = "message-id";

        var message = CreateServiceBusMessage(body, subject, id);
        var eventArgs = Substitute.For<ProcessMessageEventArgs>(message, _serviceBusReceiver, CancellationToken.None);

        var result = new ServiceBusSuccessResult();
        _webhookToDbCommand.ProcessMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                            .Returns(result);

        await _contentfulServiceBusProcessor.InvokeNonPublicAsyncMethod("MessageHandler", new object[] { eventArgs });

        await _webhookToDbCommand.Received(1).ProcessMessage(subject, body, id, Arg.Any<CancellationToken>());
        await _serviceBusResultProcessor.Received(1).ProcessMessageResult(eventArgs, result, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MessageHandler_Processes_WebhookError()
    {
        var body = "message body";
        var subject = "message subject";
        var id = "message-id";

        var message = CreateServiceBusMessage(body, subject, id);
        var eventArgs = Substitute.For<ProcessMessageEventArgs>(message, _serviceBusReceiver, CancellationToken.None);

        string errorReason = "error reason";
        string errorDescription = "error description";

        var result = new ServiceBusErrorResult(errorReason, errorDescription, false);
        _webhookToDbCommand.ProcessMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(result);

        await _contentfulServiceBusProcessor.InvokeNonPublicAsyncMethod("MessageHandler", new object[] { eventArgs });

        await _webhookToDbCommand.Received(1).ProcessMessage(subject, body, id, Arg.Any<CancellationToken>());
        await _serviceBusResultProcessor.Received(1).ProcessMessageResult(eventArgs, result, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MessageHandler_ExceptionHandling()
    {
        var body = "message body";
        var subject = "message subject";
        var id = "message-id";

        var message = CreateServiceBusMessage(body, subject, id);
        var eventArgs = Substitute.For<ProcessMessageEventArgs>(message, _serviceBusReceiver, CancellationToken.None);
        eventArgs.DeadLetterMessageAsync(message, null, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var exception = new Exception("Problem with the thing");

        _webhookToDbCommand
            .ProcessMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        await _contentfulServiceBusProcessor.InvokeNonPublicAsyncMethod("MessageHandler", new object[] { eventArgs });

        await _webhookToDbCommand.Received(1).ProcessMessage(subject, body, id, Arg.Any<CancellationToken>());
        await _serviceBusResultProcessor.Received(0).ProcessMessageResult(eventArgs, Arg.Any<IServiceBusResult>(), Arg.Any<CancellationToken>());

        await eventArgs.Received(1).DeadLetterMessageAsync(message, null, exception.Message, Arg.Any<string>(), Arg.Any<CancellationToken>());
        var loggedMessages = _logger.ReceivedLogMessages().ToArray();
        Assert.Equal(2, loggedMessages.Length);

        var matchingErrorMessage = loggedMessages.FirstOrDefault(msg => msg.LogLevel == LogLevel.Error && msg.Message.Equals($"Error processing message: {exception.Message}"));
        Assert.NotNull(matchingErrorMessage);

        var matchingAbandonMessage = loggedMessages.FirstOrDefault(msg => msg.LogLevel == LogLevel.Information && msg.Message.Equals($"Abandoned message: {message.MessageId}"));
        Assert.NotNull(matchingAbandonMessage);
    }

    [Fact]
    public async Task ErrorHandler_ShouldLogError()
    {
        var message = "Test error message";
        var exception = new Exception(message);
        var eventArgs = new ProcessErrorEventArgs(exception, ServiceBusErrorSource.AcceptSession, "", "", "", CancellationToken.None);

        await _contentfulServiceBusProcessor.InvokeNonPublicAsyncMethod("ErrorHandler", new object[] { eventArgs });

        var receivedLoggerCalls = _logger.GetMatchingReceivedMessages($"Error occurred: {message}", LogLevel.Error);
        Assert.Single(receivedLoggerCalls);
    }

    private static ServiceBusReceivedMessage CreateServiceBusMessage(string body, string subject, string id)
    {
        var message = new ServiceBusMessage(body)
        {
            Subject = subject,
            MessageId = id
        };

        var mock = Substitute.For<ServiceBusReceivedMessage>();
        var serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(message.GetRawAmqpMessage(), BinaryData.FromBytes(Encoding.UTF8.GetBytes(mock.LockToken)));
        return serviceBusReceivedMessage;
    }
}
