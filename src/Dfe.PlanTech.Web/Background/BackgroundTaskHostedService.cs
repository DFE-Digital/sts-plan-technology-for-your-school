using Dfe.PlanTech.Application.Background;

namespace Dfe.PlanTech.Web.Background;

/// <summary>
/// Reads tasks from a <see cref="IBackgroundTaskQueue"/>, and runs them.
/// </summary>
/// <param name="logger"></param>
/// <param name="taskQueue"></param>
public class BackgroundTaskHostedService(
    ILogger<BackgroundTaskHostedService> logger,
    IBackgroundTaskQueue taskQueue
) : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue =
        taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));

    /// <summary>
    /// Starts processing the queue
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting processing background tasks");
        await BackgroundProcessing(stoppingToken);
    }

    /// <summary>
    /// Processes the queue in a loop. Waits for a task to exist in the queue, reads it, and runs it.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _taskQueue.DequeueAsync(stoppingToken);

            logger.LogInformation("Read item from the queue");

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing {WorkItem}.", nameof(workItem));
            }
        }
    }

    /// <summary>
    /// Stops procesing of the queue
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping processing background tasks");

        await base.StopAsync(cancellationToken);
    }
}
