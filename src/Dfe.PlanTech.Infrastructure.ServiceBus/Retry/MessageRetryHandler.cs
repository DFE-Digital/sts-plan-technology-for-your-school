using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Retry;

/// <inheritdoc cref="IMessageRetryHandler"/>
public class MessageRetryHandler(IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory,
                                 IOptions<MessageRetryHandlingOptions> options)
    : IMessageRetryHandler
{
    private readonly ServiceBusSender _serviceBusSender = serviceBusSenderFactory.CreateClient("contentfulsender");
    private readonly MessageRetryHandlingOptions _messageRetryHandlingOptions = options.Value;

    private const string CustomMessageProperty = "DeliveryAttempts";

    /// <inheritdoc cref="IMessageRetryHandler"/>
    public async Task<bool> RetryRequired(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        var deliveryAttempts = 0;

        if (message.ApplicationProperties.TryGetValue(CustomMessageProperty, out object? attemptObj) &&
            int.TryParse(attemptObj?.ToString(), out int existingAttempt))
        {
            deliveryAttempts = existingAttempt;
        }

        if (deliveryAttempts >= _messageRetryHandlingOptions.MaxMessageDeliveryAttempts)
        {
            return false;
        }

        await RedeliverMessage(message, deliveryAttempts, cancellationToken);

        return true;
    }

    /// <summary>
    /// Creates a new copy of the service bus message, with the delivery attempt incremented, and queues this.
    /// </summary>
    /// <remarks>
    /// We create a new message to ensure it goes to the back of the queue.
    /// Some failures could be fixed by changes from other messages, so by putting it to the back of the queue we increase the chance of success.
    /// </remarks>
    /// <param name="message"></param>
    /// <param name="deliveryAttempts"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task RedeliverMessage(ServiceBusReceivedMessage message, int deliveryAttempts, CancellationToken cancellationToken)
    {
        var resubmittedMessage = new ServiceBusMessage()
        {
            ScheduledEnqueueTime = DateTime.UtcNow.AddSeconds(_messageRetryHandlingOptions.MessageDeliveryDelayInSeconds),
            Subject = message.Subject,
            Body = message.Body
        };

        ++deliveryAttempts;

        resubmittedMessage.ApplicationProperties.Add(CustomMessageProperty, deliveryAttempts);

        await _serviceBusSender.SendMessageAsync(resubmittedMessage, cancellationToken);
    }
}
