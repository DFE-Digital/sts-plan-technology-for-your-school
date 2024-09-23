using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Web.DbWriter.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter;

public class ServiceBusMessageProcessor(ServiceBusClient client, ServiceBusOptions options, ServiceBusResultProcessor resultProcessor, ILogger<ServiceBusMessageProcessor> logger, WebhookToDbCommand webhookToDbCommand) : BackgroundService
{
    private readonly ServiceBusReceiver receiver = client.CreateReceiver(options.QueueName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await ProcessContentfulMessageQueue(cancellationToken);
    }

    public async Task ProcessContentfulMessageQueue(CancellationToken cancellationToken)
    {
        logger.LogInformation("Beginning Contentful update processing");
        var messages = await GetMessages(cancellationToken);
        logger.LogInformation("Found {MessageCount} messages to process", messages.Count);

        foreach (var message in messages)
        {
            await MessageHandler(message, cancellationToken);
        }

        logger.LogInformation("Finished Contentful update processing");
    }

    protected async Task<IReadOnlyList<ServiceBusReceivedMessage>> GetMessages(CancellationToken cancellationToken)
    {
        try
        {
            var messages = await receiver.ReceiveMessagesAsync(maxMessages: options.MessagesPerBatch, cancellationToken: cancellationToken);
            return messages;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading messages from queue");
            return [];
        }
    }

    private async Task MessageHandler(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var result = await webhookToDbCommand.ProcessMessage(message, cancellationToken);
            await resultProcessor.ProcessMessageResult(receiver, message, result, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message: {Message}", ex.Message);
            await receiver.DeadLetterMessageAsync(message);
            logger.LogInformation("Abandoned message: {MessageId}", message.MessageId);
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError("Error occurred: {Message}", args.Exception.Message);
        return Task.CompletedTask;
    }
}
