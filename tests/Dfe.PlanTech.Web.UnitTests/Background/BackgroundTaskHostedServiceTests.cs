using Dfe.PlanTech.Application.Background;
using Dfe.PlanTech.Web.Background;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Background;

public class BackgroundTaskHostedServiceTests
{
    [Fact]
    public async Task ExecuteAsync_Runs_One_WorkItem_And_Logs()
    {
        var logger = Substitute.For<ILogger<BackgroundTaskHostedService>>();
        var queue = Substitute.For<IBackgroundTaskQueue>();

        using var cts = new CancellationTokenSource();
        var workItemCompleted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        queue
            .DequeueAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Func<CancellationToken, Task>>(async ct =>
                {
                    try
                    {
                        await Task.CompletedTask;
                    }
                    finally
                    {
                        cts.Cancel();
                        workItemCompleted.TrySetResult();
                    }
                })
            );

        var sut = new BackgroundTaskHostedService(logger, queue);

        await sut.StartAsync(cts.Token);

        await workItemCompleted.Task.WaitAsync(
            TimeSpan.FromSeconds(2),
            TestContext.Current.CancellationToken
        );

        await queue.Received(1).DequeueAsync(Arg.Any<CancellationToken>());

        var infoLogs = logger
            .ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(ILogger.Log))
            .Where(c =>
                c.GetArguments().Length > 0
                && c.GetArguments()[0] is LogLevel lvl
                && lvl == LogLevel.Information
            )
            .ToList();

        Assert.True(infoLogs.Count >= 2, $"Expected >= 2 info logs, got {infoLogs.Count}");
    }

    [Fact]
    public async Task ExecuteAsync_When_WorkItem_Throws_Logs_Error_And_Continues()
    {
        // Arrange
        var logger = Substitute.For<ILogger<BackgroundTaskHostedService>>();
        var queue = Substitute.For<IBackgroundTaskQueue>();
        using var cts = new CancellationTokenSource();

        var secondItemRan = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        queue
            .DequeueAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Func<CancellationToken, Task>>(_ =>
                    throw new InvalidOperationException("boom")
                ),
                Task.FromResult<Func<CancellationToken, Task>>(ct =>
                {
                    cts.Cancel(); // stop after processing the second item
                    secondItemRan.TrySetResult();
                    return Task.CompletedTask;
                })
            );

        var sut = new BackgroundTaskHostedService(logger, queue);

        // Act
        await sut.StartAsync(cts.Token);

        await secondItemRan.Task.WaitAsync(
            TimeSpan.FromSeconds(2),
            TestContext.Current.CancellationToken
        );

        // Assert: both dequeues happened
        await queue.Received(2).DequeueAsync(Arg.Any<CancellationToken>());

        // Assert: an error log was written
        var errorLogs = logger
            .ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(ILogger.Log))
            .Where(c =>
                c.GetArguments().Length > 0
                && c.GetArguments()[0] is LogLevel lvl
                && lvl == LogLevel.Error
            )
            .ToList();

        Assert.NotEmpty(errorLogs);
    }

    [Fact]
    public async Task ExecuteAsync_With_AlreadyCancelledToken_Does_Not_Dequeue()
    {
        var logger = Substitute.For<ILogger<BackgroundTaskHostedService>>();
        var queue = Substitute.For<IBackgroundTaskQueue>();
        var sut = new BackgroundTaskHostedService(logger, queue);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await sut.StartAsync(cts.Token);

        await queue.DidNotReceive().DequeueAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Passes_Same_CancellationToken_To_DequeueAsync()
    {
        var logger = Substitute.For<ILogger<BackgroundTaskHostedService>>();
        var queue = Substitute.For<IBackgroundTaskQueue>();
        var sut = new BackgroundTaskHostedService(logger, queue);

        using var cts = new CancellationTokenSource();

        var dequeueCalled = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        CancellationToken captured = default;
        var wasCancelledBeforeSourceCancel = false;

        queue
            .DequeueAsync(Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                captured = callInfo.Arg<CancellationToken>();
                wasCancelledBeforeSourceCancel = captured.IsCancellationRequested;

                cts.Cancel();

                dequeueCalled.TrySetResult(true);

                return Task.FromResult<Func<CancellationToken, Task>>(_ => Task.CompletedTask);
            });

        await sut.StartAsync(cts.Token);

        await dequeueCalled.Task.WaitAsync(
            TimeSpan.FromSeconds(2),
            TestContext.Current.CancellationToken
        );

        Assert.False(wasCancelledBeforeSourceCancel);
        Assert.True(captured.CanBeCanceled);
        Assert.True(captured.IsCancellationRequested);
        Assert.Equal(cts.Token.IsCancellationRequested, captured.IsCancellationRequested);
    }

    [Fact]
    public async Task StopAsync_Logs_Stopping_Message()
    {
        var logger = Substitute.For<ILogger<BackgroundTaskHostedService>>();
        var queue = Substitute.For<IBackgroundTaskQueue>();
        var sut = new BackgroundTaskHostedService(logger, queue);

        await sut.StopAsync(CancellationToken.None);

        logger.ReceivedWithAnyArgs().Log(LogLevel.Information, 0, default!, null, default!);
    }
}
