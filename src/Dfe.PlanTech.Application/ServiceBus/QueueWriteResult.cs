namespace Dfe.PlanTech.Domain.Queues.Models;

public class QueueWriteResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public QueueWriteResult(bool success, string? errorMessage = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public QueueWriteResult(string errorMessage)
        : this(false, errorMessage) { }
}
