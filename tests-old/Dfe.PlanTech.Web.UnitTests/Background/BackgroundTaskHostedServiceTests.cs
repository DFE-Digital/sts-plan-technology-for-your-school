using Dfe.PlanTech.Application.Background;
using Dfe.PlanTech.Domain.Background;
using Dfe.PlanTech.Web.Background;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Background;
public class BackgroundTaskHostedServiceTests
{
    private readonly ILogger<BackgroundTaskHostedService> logger = Substitute.For<ILogger<BackgroundTaskHostedService>>();
    private readonly IOptions<BackgroundTaskQueueOptions> options = Substitute.For<IOptions<BackgroundTaskQueueOptions>>();
    private readonly BackgroundTaskQueue _taskQueue;
    private readonly BackgroundTaskHostedService _service;
    private string _mockResult = "";
    private readonly string _expectedResult = "Test has ran";

    public BackgroundTaskHostedServiceTests()
    {
        options.Value.Returns(new BackgroundTaskQueueOptions());
        _taskQueue = new BackgroundTaskQueue(options);
        _service = new BackgroundTaskHostedService(logger, _taskQueue);
    }

    [Fact]
    public async Task Should_Read_From_Queue()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(2000);
        var cancellationToken = cancellationTokenSource.Token;

        await Task.WhenAll(_taskQueue.QueueBackgroundWorkItemAsync(ct =>
        {
            _mockResult = _expectedResult;
            return Task.CompletedTask;
        }),
        _service.StartAsync(cancellationToken));

        Assert.Equal(_expectedResult, _mockResult);
        cancellationTokenSource.Dispose();

        var loggedMessages = logger.ReceivedLogMessages().ToArray();
        Assert.Contains(loggedMessages, message => message.Message.Equals("Starting processing background tasks") && message.LogLevel == LogLevel.Information);
        Assert.Contains(loggedMessages, message => message.Message.Equals("Read item from the queue") && message.LogLevel == LogLevel.Information);
    }

    [Fact]
    public async Task BackgroundProcessing_ShouldLogError_WhenWorkItemFails()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(2000);
        var cancellationToken = cancellationTokenSource.Token;

        await Task.WhenAll(_taskQueue.QueueBackgroundWorkItemAsync(ct =>
        {
            throw new Exception("An error occurred with the task");
        }),
        _service.StartAsync(cancellationToken));
        cancellationTokenSource.Dispose();

        var loggedMessages = logger.ReceivedLogMessages().ToArray();

        Assert.Contains(loggedMessages, message => message.Message.Equals("Starting processing background tasks") && message.LogLevel == LogLevel.Information);
        Assert.Contains(loggedMessages, message => message.Message.Equals("Read item from the queue") && message.LogLevel == LogLevel.Information);
        Assert.Contains(loggedMessages, message => message.Message.StartsWith("Error occurred executing") && message.LogLevel == LogLevel.Error);
    }

    [Fact]
    public async Task StopAsync_ShouldLogStoppingMessage()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        await _service.StopAsync(cancellationToken);

        // Assert
        Assert.Contains(logger.ReceivedLogMessages(), message => message.Message.Equals("Stopping processing background tasks") && message.LogLevel == LogLevel.Information);
    }
}
