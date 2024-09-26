namespace Dfe.PlanTech.Application.Queues.Interfaces;

public interface IQueueWriter
{
    public Task WriteMessage(string body, string subject);
}
