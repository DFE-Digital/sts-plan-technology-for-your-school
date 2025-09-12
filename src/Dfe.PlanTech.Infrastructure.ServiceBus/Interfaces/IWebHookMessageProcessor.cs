namespace Dfe.PlanTech.Infrastructure.ServiceBus.Interfaces;

public interface IWebHookMessageProcessor
{
    Task<IServiceBusResult> ProcessMessage(string subject, string body, string id, CancellationToken cancellationToken);
}
