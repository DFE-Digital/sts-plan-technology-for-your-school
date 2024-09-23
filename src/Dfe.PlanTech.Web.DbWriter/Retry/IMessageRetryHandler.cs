using Azure.Messaging.ServiceBus;

namespace Dfe.PlanTech.Web.DbWriter.Retry;

public interface IMessageRetryHandler
{
    Task<bool> RetryRequired(ServiceBusReceivedMessage message, CancellationToken cancellationToken);
}
