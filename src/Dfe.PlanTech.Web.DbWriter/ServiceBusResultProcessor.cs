using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Web.DbWriter.Retry;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter;

public class ServiceBusResultProcessor(IMessageRetryHandler retryHandler, ILogger<ServiceBusResultProcessor> logger)
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
                case ServiceBusDeadLetterResult deadLetterResult:
                    await ProcessDeadLetterResult(processMessageEventArgs, deadLetterResult, cancellationToken);
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

    private async Task ProcessDeadLetterResult(ProcessMessageEventArgs processMessageEventArgs, ServiceBusDeadLetterResult deadLetterResult, CancellationToken cancellationToken)
    {
        var shouldRetry = await retryHandler.RetryRequired(processMessageEventArgs.Message, cancellationToken);

        if (shouldRetry)
        {
            logger.LogWarning("Error processing message ID {MessageId}.\n{Reason}\n{Description}\nWill retry again", processMessageEventArgs.Message.MessageId, deadLetterResult.Reason, deadLetterResult.Description ?? "");
            await processMessageEventArgs.CompleteMessageAsync(processMessageEventArgs.Message, cancellationToken);
            return;
        }

        logger.LogError("Error processing message ID {MessageId}.\n{Reason}\n{Description}\nThe maximum delivery count has been reached", processMessageEventArgs.Message.MessageId, deadLetterResult.Reason, deadLetterResult.Description ?? "");
        await processMessageEventArgs.DeadLetterMessageAsync(processMessageEventArgs.Message, null, deadLetterResult.Reason, deadLetterResult.Description, cancellationToken);
    }
}
