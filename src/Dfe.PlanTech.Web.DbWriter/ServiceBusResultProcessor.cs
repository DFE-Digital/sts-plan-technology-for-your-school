using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Dfe.PlanTech.Web.DbWriter.Retry;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter;

public class ServiceBusResultProcessor(IMessageRetryHandler retryHandler, ILogger<ServiceBusResultProcessor> logger)
{
  public async Task ProcessMessageResult(ProcessMessageEventArgs processMessageEventArgs, ServiceBusReceivedMessage message, IServiceBusResult result, CancellationToken cancellationToken)
  {
    try
    {
      switch (result)
      {
        case ServiceBusSuccessResult:
          await ProcessSuccessResult(processMessageEventArgs, message, cancellationToken);
          break;
        case ServiceBusDeadLetterResult deadLetterResult:
          await ProcessDeadLetterResult(processMessageEventArgs, message, deadLetterResult, cancellationToken);
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

  private static async Task ProcessSuccessResult(ProcessMessageEventArgs processMessageEventArgs, ServiceBusReceivedMessage message, CancellationToken cancellationToken) => await processMessageEventArgs.CompleteMessageAsync(message, cancellationToken);

  private async Task ProcessDeadLetterResult(ProcessMessageEventArgs processMessageEventArgs, ServiceBusReceivedMessage message, ServiceBusDeadLetterResult deadLetterResult, CancellationToken cancellationToken)
  {
    var shouldRetry = await retryHandler.RetryRequired(message, cancellationToken);

    if (shouldRetry)
    {
      logger.LogWarning("Error processing message ID {MessageId}.\n{Reason}\n{Description}\nWill retry again", message.MessageId, deadLetterResult.Reason, deadLetterResult.Description ?? "");
      await processMessageEventArgs.CompleteMessageAsync(message, cancellationToken);
      return;
    }

    logger.LogError("Error processing message ID {MessageId}.\n{Reason}\n{Description}\nThe maximum delivery count has been reached", message.MessageId, deadLetterResult.Reason, deadLetterResult.Description ?? "");
    await processMessageEventArgs.DeadLetterMessageAsync(message, null, deadLetterResult.Reason, deadLetterResult.Description, cancellationToken);
  }
}