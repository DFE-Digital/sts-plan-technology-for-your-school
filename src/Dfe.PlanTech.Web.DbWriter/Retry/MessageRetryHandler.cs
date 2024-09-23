using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.DbWriter.Retry;

public class MessageRetryHandler(
    IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory,
    IOptions<MessageRetryHandlingOptions> options)
    : IMessageRetryHandler
{
    private readonly ServiceBusSender _serviceBusSender = serviceBusSenderFactory.CreateClient("contentfulsender");
    private readonly MessageRetryHandlingOptions _messageRetryHandlingOptions = options.Value;

    private const string CustomMessageProperty = "DeliveryAttempts";

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
