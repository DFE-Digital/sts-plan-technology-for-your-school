using Azure.Messaging.ServiceBus;

namespace Dfe.PlanTech.AzureFunctions.Utils;

public interface IMessageRetryHandler
{
    Task<bool> RetryRequired(ServiceBusReceivedMessage message, CancellationToken cancellationToken);
}
