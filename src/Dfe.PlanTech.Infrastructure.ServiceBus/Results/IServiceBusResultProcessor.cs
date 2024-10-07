using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.ServiceBus.Models;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Results;

public interface IServiceBusResultProcessor
{
    public Task ProcessMessageResult(ProcessMessageEventArgs processMessageEventArgs, IServiceBusResult result,
        CancellationToken cancellationToken);
}
