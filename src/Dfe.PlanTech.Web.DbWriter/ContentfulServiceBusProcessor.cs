using System.Text;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Persistence.Commands;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter;

/// <summary>
/// Processes messages from Service Bus, and saves them into the DB using the <see cref="WebhookToDbCommand"/>.
/// </summary>
/// <param name="processorFactory"></param>
/// <param name="resultProcessor">Processes results from the <see cref="WebhookToDbCommand"/> </param>
/// <param name="logger"></param>
/// <param name="webhookToDbCommand">Processes Contentful webhook update payloads, and saves to the DB if appropriate</param>
public class ContentfulServiceBusProcessor(IAzureClientFactory<ServiceBusProcessor> processorFactory,
                                           ServiceBusResultProcessor resultProcessor,
                                           ILogger<ContentfulServiceBusProcessor> logger,
                                           WebhookToDbCommand webhookToDbCommand) : BackgroundService
{
    private readonly ServiceBusProcessor processor = processorFactory.CreateClient("contentfulprocessor");

    /// <summary>
    /// Adds event handlers for the message received event + error event
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;

        await processor.StartProcessingAsync(stoppingToken);
        stoppingToken.Register(async () => await StopProcessingAsync());
    }

    /// <summary>
    /// Receives messages from the <see cref="ServiceBusProcessor"/>, saves them to the DB using the <see cref="WebhookToDbCommand"/>, and then processses the results with <see cref="ServiceBusResultProcessor"/> 
    /// </summary>
    /// <param name="processMessageEventArgs">Received Service Bus message</param>
    private async Task MessageHandler(ProcessMessageEventArgs processMessageEventArgs)
    {
        try
        {
            var body = Encoding.UTF8.GetString(processMessageEventArgs.Message.Body);
            var result = await webhookToDbCommand.ProcessMessage(processMessageEventArgs.Message.Subject, body, processMessageEventArgs.Message.MessageId, processMessageEventArgs.CancellationToken);
            await resultProcessor.ProcessMessageResult(processMessageEventArgs, result, processMessageEventArgs.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message: {Message}", ex.Message);
            await processMessageEventArgs.DeadLetterMessageAsync(processMessageEventArgs.Message, null, ex.Message, ex.StackTrace, processMessageEventArgs.CancellationToken);
            logger.LogInformation("Abandoned message: {MessageId}", processMessageEventArgs.Message);
        }
    }

    /// <summary>
    /// Handles uncaught errors processing a message
    /// </summary>
    /// <remarks>
    /// _Shouldn't_ be needed, as the entire MessageHandler is wrapped in a try/catch (plus all the other exception handling elsewhere)
    /// </remarks>
    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError("Error occurred: {Message}", args.Exception.Message);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Runs when the cancellation token in ExecuteAsync is requesting a stop. Stops the processing of messages, and disposes of the processor.
    /// </summary>
    /// <returns></returns>
    private async Task StopProcessingAsync()
    {
        await processor.StopProcessingAsync();
        await processor.DisposeAsync();
        logger.LogInformation("Stopped processing messages.");
    }
}
