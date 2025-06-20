using System.Text;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Persistence.Commands;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Infrastructure.ServiceBus.Results;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Infrastructure.ServiceBus;

/// <summary>
/// Processes messages from Service Bus, and saves them into the DB using the <see cref="CmsWebHookMessageProcessor"/>.
/// </summary>
/// <param name="processorFactory"></param>
/// <param name="resultProcessor">Processes results from the <see cref="CmsWebHookMessageProcessor"/> </param>
/// <param name="logger"></param>
/// <param name="serviceScopeFactory">Service factory - used to create transient services to prevent state problems</param>
public class ContentfulServiceBusProcessor(IAzureClientFactory<ServiceBusProcessor> processorFactory,
                                           IServiceBusResultProcessor resultProcessor,
                                           ILogger<ContentfulServiceBusProcessor> logger,
                                           IServiceScopeFactory serviceScopeFactory,
                                           IOptions<ServiceBusOptions> options) : BackgroundService
{
    private readonly ServiceBusProcessor _processor = processorFactory.CreateClient("contentfulprocessor");

    /// <summary>
    /// Adds event handlers for the message received event + error event
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.EnableQueueReading)
        {
            logger.LogInformation("{QueueReadingProperty} is set to disabling - not enabling processing queue reading", nameof(options.Value.EnableQueueReading));
            return;
        }
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        await _processor.StartProcessingAsync(stoppingToken);

        stoppingToken.Register(() => StopProcessingAsync().GetAwaiter().GetResult());
    }

    /// <summary>
    /// Receives messages from the <see cref="ServiceBusProcessor"/>, saves them to the DB using the <see cref="CmsWebHookMessageProcessor"/>, and then processses the results with <see cref="ServiceBusResultProcessor"/>
    /// </summary>
    /// <param name="processMessageEventArgs">Received Service Bus message</param>
    private async Task MessageHandler(ProcessMessageEventArgs processMessageEventArgs)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var webhookToDbCommand = scope.ServiceProvider.GetRequiredService<IContentfulMessageProcessor>();

        try
        {
            var body = Encoding.UTF8.GetString(processMessageEventArgs.Message.Body);

            var result = await webhookToDbCommand.ProcessMessage(processMessageEventArgs.Message.Subject,
                                                                 body,
                                                                 processMessageEventArgs.Message.MessageId,
                                                                 processMessageEventArgs.CancellationToken);

            await resultProcessor.ProcessMessageResult(processMessageEventArgs,
                                                       result,
                                                       processMessageEventArgs.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message: {Message}", ex.Message);
            await processMessageEventArgs.DeadLetterMessageAsync(processMessageEventArgs.Message, null, ex.Message,
                ex.StackTrace, processMessageEventArgs.CancellationToken);
            logger.LogInformation("Abandoned message: {MessageId}", processMessageEventArgs.Message.MessageId);
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
        await _processor.StopProcessingAsync();
        await _processor.DisposeAsync();
        logger.LogInformation("Stopped processing messages.");
    }
}
