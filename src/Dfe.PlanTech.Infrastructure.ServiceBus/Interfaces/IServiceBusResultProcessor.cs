using Azure.Messaging.ServiceBus;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Interfaces
{
    public interface IServiceBusResultProcessor
    {
        Task ProcessMessageResult(
            ProcessMessageEventArgs processMessageEventArgs,
            IServiceBusResult result,
            CancellationToken cancellationToken
        );
    }
}
