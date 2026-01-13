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
        // Arrange
        var logger = Substitute.For<ILogger<BackgroundTaskHostedService>>();
        var queue = Substitute.For<IBackgroundTaskQueue>();

        var ran = false;
        var cts = new CancellationTokenSource();

        // Dequeue once, then cancel so the loop exits
        queue.DequeueAsync(Arg.Any<CancellationToken>())
             .Returns(Task.FromResult<Func<CancellationToken, Task>>(ct =>
             {
                 ran = true;
                 cts.Cancel(); // stop the loop after this item
                 return Task.CompletedTask;
             }));

        var sut = new BackgroundTaskHostedService(logger, queue);

        // Act
        await sut.StartAsync(cts.Token); // StartAsync calls ExecuteAsync under the hood

        // Assert
        Assert.True(ran);
        await queue.Received(1).DequeueAsync(Arg.Any<CancellationToken>());

        // At least two info logs: starting + read item
        var infoLogs = logger.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(ILogger.Log))
            .Where(c => c.GetArguments().Length > 0 && c.GetArguments()[0] is LogLevel lvl && lvl == LogLevel.Information)
            .ToList();

        Assert.True(infoLogs.Count >= 2, $"Expected >= 2 info logs, got {infoLogs.Count}");
    }

    [Fact]
    public async Task ExecuteAsync_When_WorkItem_Throws_Logs_Error_And_Continues()
    {
        // Arrange
        var logger = Substitute.For<ILogger<BackgroundTaskHostedService>>();
        var queue = Substitute.For<IBackgroundTaskQueue>();
        var cts = new CancellationTokenSource();

        // First dequeued item throws
        queue.DequeueAsync(Arg.Any<CancellationToken>())
             .Returns(
                 Task.FromResult<Func<CancellationToken, Task>>(_ => throw new InvalidOperationException("boom")),
                 Task.FromResult<Func<CancellationToken, Task>>(ct =>
                 {
                     cts.Cancel(); // stop after processing the second item
                     return Task.CompletedTask;
                 })
             );

        var sut = new BackgroundTaskHostedService(logger, queue);

        // Act
        await sut.StartAsync(cts.Token);

        // Assert: error was logged at least once
        logger.ReceivedWithAnyArgs().Log(LogLevel.Error, 0, default!, Arg.Any<Exception>(), default!);

        // Both dequeues happened (first threw, second cancelled the loop)
        await queue.Received(2).DequeueAsync(Arg.Any<CancellationToken>());
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

        var cts = new CancellationTokenSource();

        CancellationToken? captured = null;

        queue.DequeueAsync(Arg.Any<CancellationToken>())
             .Returns(ci =>
             {
                 captured = ci.Arg<CancellationToken>();
                 cts.Cancel();
                 return Task.FromResult<Func<CancellationToken, Task>>(_ => Task.CompletedTask);
             });

        await sut.StartAsync(cts.Token);

        Assert.True(captured.HasValue);
        Assert.Equal(cts.Token.ToString(), captured!.Value.ToString());
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
