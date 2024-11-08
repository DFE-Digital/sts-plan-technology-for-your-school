using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Application.Persistence.Commands;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Retry;

/// <summary>
/// Handles Service Bus messages that the <see cref="WebhookMessageProcessor"/> failed to process; will re-queue the message if not exceeded retry count
/// </summary>
public interface IMessageRetryHandler
{
    /// <summary>
    /// Checks how many times the message has already been attempted. If below the count then it will redeliver it
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if retry was required, false if not</returns>
    Task<bool> RetryRequired(ServiceBusReceivedMessage message, CancellationToken cancellationToken);
}
