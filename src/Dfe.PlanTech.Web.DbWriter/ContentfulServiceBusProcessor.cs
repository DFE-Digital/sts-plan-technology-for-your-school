using System.Text;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Persistence.Commands;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter;

public class ContentfulServiceBusProcessor(IAzureClientFactory<ServiceBusProcessor> processorFactory,
                                           ServiceBusResultProcessor resultProcessor,
                                           ILogger<ContentfulServiceBusProcessor> logger,
                                           WebhookToDbCommand webhookToDbCommand) : BackgroundService
{
    private CancellationToken cancellationToken = default;
    private readonly ServiceBusProcessor processor = processorFactory.CreateClient("contentfulprocessor");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        cancellationToken = stoppingToken;
        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;

        await processor.StartProcessingAsync(cancellationToken);
        cancellationToken.Register(async () => await StopProcessingAsync());
    }

    private async Task MessageHandler(ProcessMessageEventArgs processMessageEventArgs)
    {
        try
        {
            var body = Encoding.UTF8.GetString(processMessageEventArgs.Message.Body);
            var result = await webhookToDbCommand.ProcessMessage(processMessageEventArgs.Message.Subject, body, processMessageEventArgs.Message.MessageId, cancellationToken);
            await resultProcessor.ProcessMessageResult(processMessageEventArgs, processMessageEventArgs.Message, result, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message: {Message}", ex.Message);
            await processMessageEventArgs.DeadLetterMessageAsync(processMessageEventArgs.Message, null, ex.Message, ex.StackTrace, CancellationToken.None);
            logger.LogInformation("Abandoned message: {MessageId}", processMessageEventArgs.Message);
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError("Error occurred: {Message}", args.Exception.Message);
        return Task.CompletedTask;
    }

    private async Task StopProcessingAsync()
    {
        await processor.StopProcessingAsync();
        await processor.DisposeAsync();
        logger.LogInformation("Stopped processing messages.");
    }
}
