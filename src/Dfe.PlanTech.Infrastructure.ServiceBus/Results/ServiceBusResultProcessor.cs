using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Infrastructure.ServiceBus.Retries;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Results;

public class ServiceBusResultProcessor(IMessageRetryHandler retryHandler, ILogger<ServiceBusResultProcessor> logger) : IServiceBusResultProcessor
{
    public async Task ProcessMessageResult(ProcessMessageEventArgs processMessageEventArgs, IServiceBusResult result, CancellationToken cancellationToken)
    {
        try
        {
            switch (result)
            {
                case ServiceBusSuccessResult:
                    await ProcessSuccessResult(processMessageEventArgs, cancellationToken);
                    break;
                case ServiceBusErrorResult deadLetterResult:
                    await ProcessErrorResult(processMessageEventArgs, deadLetterResult, cancellationToken);
                    break;
                default:
                    logger.LogError("Unexpected service bus result type: {Type}", result.GetType().Name);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message result: {Message}", ex.Message);
        }
    }

    private static async Task ProcessSuccessResult(ProcessMessageEventArgs processMessageEventArgs, CancellationToken cancellationToken)
      => await processMessageEventArgs.CompleteMessageAsync(processMessageEventArgs.Message, cancellationToken);

    private async Task ProcessErrorResult(ProcessMessageEventArgs processMessageEventArgs, ServiceBusErrorResult errorResult, CancellationToken cancellationToken)
    {
        var shouldRetry = await retryHandler.RetryRequired(processMessageEventArgs.Message, cancellationToken);

        if (shouldRetry)
        {
            logger.LogWarning("Error processing message ID {MessageId}.\n{Reason}\n{Description}\nWill retry again", processMessageEventArgs.Message.MessageId, errorResult.Reason, errorResult.Description ?? "");
            await processMessageEventArgs.CompleteMessageAsync(processMessageEventArgs.Message, cancellationToken);
            return;
        }

        logger.LogError("Error processing message ID {MessageId}.\n{Reason}\n{Description}\nThe maximum delivery count has been reached", processMessageEventArgs.Message.MessageId, errorResult.Reason, errorResult.Description ?? "");
        await processMessageEventArgs.DeadLetterMessageAsync(processMessageEventArgs.Message, null, errorResult.Reason, errorResult.Description, cancellationToken);
    }
}
