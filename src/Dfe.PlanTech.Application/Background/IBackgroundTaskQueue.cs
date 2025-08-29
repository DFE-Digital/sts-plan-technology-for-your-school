namespace Dfe.PlanTech.Application.Background;

/// <summary>
/// Queue for tasks to be ran in background
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// Add an async operation to the queue for background processing. 
    /// </summary>
    /// <param name="workItem"></param>
    /// <returns></returns>
    Task QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem);

    /// <summary>
    /// Removes an item from the queue
    /// </summary>
    /// <remarks>
    /// Will wait until an item exists
    /// </remarks>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}
