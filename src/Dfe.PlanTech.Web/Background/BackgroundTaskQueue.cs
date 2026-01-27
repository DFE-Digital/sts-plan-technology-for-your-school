using System.Threading.Channels;
using Dfe.PlanTech.Application.Background;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.Background;

/// <inheritdoc cref="IBackgroundTaskQueue" />
public class BackgroundTaskQueue(IOptions<BackgroundTaskQueueOptions> options)
    : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue = Channel.CreateBounded<
        Func<CancellationToken, Task>
    >(CreateChannelOptions(options.Value));

    /// <inheritdoc cref="IBackgroundTaskQueue" />
    public async Task QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        await _queue.Writer.WriteAsync(workItem);
    }

    /// <inheritdoc cref="IBackgroundTaskQueue" />
    public async Task<Func<CancellationToken, Task>> DequeueAsync(
        CancellationToken cancellationToken
    )
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);

        return workItem;
    }

    private static BoundedChannelOptions CreateChannelOptions(BackgroundTaskQueueOptions options) =>
        new(options.MaxQueueSize) { FullMode = options.FullMode };
}
