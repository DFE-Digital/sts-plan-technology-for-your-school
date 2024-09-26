namespace Dfe.PlanTech.Domain.Queues.Models;

public record QueueWriteResult(bool Success, string? ErrorMessage = null)
{
    public QueueWriteResult(string errorMessage) : this(false, errorMessage) { }
}
