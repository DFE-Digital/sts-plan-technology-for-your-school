using Dfe.PlanTech.Domain.Queues.Models;

namespace Dfe.PlanTech.Application.Queues.Interfaces;

/// <summary>
/// Writes messages to pub/sub queues
/// </summary>
public interface IQueueWriter
{
    public Task<QueueWriteResult> WriteMessage(string body, string subject);
}
