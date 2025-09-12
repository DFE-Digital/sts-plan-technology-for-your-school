using Dfe.PlanTech.Infrastructure.ServiceBus.Queueing;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Interfaces;

/// <summary>
/// Writes messages to pub/sub queues
/// </summary>
public interface IQueueWriter
{
    public Task<QueueWriteResult> WriteMessage(string body, string subject);
}
