using System.Threading.Channels;
using Dfe.PlanTech.Application.Background;
using Dfe.PlanTech.Web.Background;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.UnitTests.Background;

public class BackgroundTaskQueueTests
{
    private static BackgroundTaskQueue NewQueue(int size = 10, BoundedChannelFullMode mode = BoundedChannelFullMode.Wait)
    {
        var opts = Options.Create(new BackgroundTaskQueueOptions
        {
            MaxQueueSize = size,
            FullMode = mode
        });
        return new BackgroundTaskQueue(opts);
    }

    [Fact]
    public async Task QueueBackgroundWorkItemAsync_Throws_On_Null()
    {
        var q = NewQueue();
        await Assert.ThrowsAsync<ArgumentNullException>(() => q.QueueBackgroundWorkItemAsync(null!));
    }

    [Fact]
    public async Task Queue_Then_Dequeue_Returns_Same_Delegate_And_Executes()
    {
        var q = NewQueue();

        var ran = false;
        Func<CancellationToken, Task> work = _ =>
        {
            ran = true;
            return Task.CompletedTask;
        };

        await q.QueueBackgroundWorkItemAsync(work);
        var dequeued = await q.DequeueAsync(CancellationToken.None);

        Assert.Same(work, dequeued);

        // Execute it to ensure it runs without issue
        await dequeued(CancellationToken.None);
        Assert.True(ran);
    }

    [Fact]
    public async Task DequeueAsync_Respects_CancellationToken()
    {
        var q = NewQueue(size: 1);

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // already canceled

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            // ReadAsync on the underlying channel throws when token is canceled
            await q.DequeueAsync(cts.Token);
        });
    }

    [Fact]
    public async Task Queue_Is_FIFO_When_Wait_Mode()
    {
        var q = NewQueue(size: 10, mode: BoundedChannelFullMode.Wait);

        var order = new List<int>();

        Func<CancellationToken, Task> a = _ => { order.Add(1); return Task.CompletedTask; };
        Func<CancellationToken, Task> b = _ => { order.Add(2); return Task.CompletedTask; };

        await q.QueueBackgroundWorkItemAsync(a);
        await q.QueueBackgroundWorkItemAsync(b);

        var d1 = await q.DequeueAsync(CancellationToken.None);
        var d2 = await q.DequeueAsync(CancellationToken.None);

        Assert.Same(a, d1);
        Assert.Same(b, d2);

        await d1(CancellationToken.None);
        await d2(CancellationToken.None);

        Assert.Equal(new[] { 1, 2 }, order);
    }

    [Fact]
    public async Task When_DropOldest_And_Capacity_1_Second_Enqueue_Drops_First()
    {
        // capacity 1, DropOldest -> writing B after A should evict A; first dequeue returns B
        var q = NewQueue(size: 1, mode: BoundedChannelFullMode.DropOldest);

        Func<CancellationToken, Task> a = _ => Task.CompletedTask;
        Func<CancellationToken, Task> b = _ => Task.CompletedTask;

        await q.QueueBackgroundWorkItemAsync(a);
        await q.QueueBackgroundWorkItemAsync(b);

        var d = await q.DequeueAsync(CancellationToken.None);
        Assert.Same(b, d);
    }
}
