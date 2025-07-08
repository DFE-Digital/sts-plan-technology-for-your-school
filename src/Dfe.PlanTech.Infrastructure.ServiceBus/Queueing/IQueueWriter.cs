namespace Dfe.PlanTech.Infrastructure.ServiceBus.Queueing;

/// <summary>
/// Writes messages to pub/sub queues
/// </summary>
public interface IQueueWriter
{
    public Task<QueueWriteResult> WriteMessage(string body, string subject);
}
