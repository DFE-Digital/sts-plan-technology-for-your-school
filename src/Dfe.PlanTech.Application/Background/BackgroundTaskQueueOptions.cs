using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace Dfe.PlanTech.Domain.Background;

/// <summary>
/// Options for <see cref="IBackgroundTaskQueue"/> 
/// </summary>
/// <param name="MaxQueueSize">Maximum number of tasks that can be enqueued before the queue is full. Defaults to 10.</param>
/// <param name="FullMode">What to do when the queue is full. Defaults to wait. See <see cref="BoundedChannelFullMode" /> for more details.</param>
[ExcludeFromCodeCoverage]
public record BackgroundTaskQueueOptions(int MaxQueueSize, BoundedChannelFullMode FullMode)
{
    public BackgroundTaskQueueOptions() : this(10, BoundedChannelFullMode.Wait) { }
}
