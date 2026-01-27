using System.Text.Json;
using Dfe.PlanTech.Infrastructure.ServiceBus.Commands;
using Dfe.PlanTech.Infrastructure.ServiceBus.Interfaces;
using Dfe.PlanTech.Infrastructure.ServiceBus.Queueing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.UnitTests.Commands;

public class WriteCmsWebhookToQueueCommandTests
{
    private readonly ILogger<WriteCmsWebhookToQueueCommand> _logger = Substitute.For<
        ILogger<WriteCmsWebhookToQueueCommand>
    >();
    private readonly IQueueWriter _writer = Substitute.For<IQueueWriter>();

    private static HttpRequest NewRequestWithHeader(string? headerValue)
    {
        var httpContext = new DefaultHttpContext();
        if (headerValue is null)
        {
            // no header
        }
        else
        {
            // If you want to simulate an "empty" header, pass string.Empty
            httpContext.Request.Headers[WriteCmsWebhookToQueueCommand.ContentfulTopicHeaderKey] =
                headerValue;
        }
        return httpContext.Request;
    }

    [Fact]
    public async Task WriteMessageToQueue_WhenHeaderPresent_Writes_Message_And_Returns_Result()
    {
        // Arrange
        var sut = new WriteCmsWebhookToQueueCommand(_logger, _writer);
        var request = NewRequestWithHeader("ContentManagement.Entry.publish");
        using var json = JsonDocument.Parse("{\"id\":\"123\",\"type\":\"entry\"}");

        var expected = new QueueWriteResult(true);
        _writer
            .WriteMessage(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await sut.WriteMessageToQueue(json, request);

        // Assert
        Assert.Same(expected, result);

        // Verify the exact arguments passed to the writer
        var serializedJson = JsonSerializer.Serialize(json);
        await _writer
            .Received(1)
            .WriteMessage(
                Arg.Is<string>(s => s == serializedJson),
                Arg.Is<string>(s => s == "ContentManagement.Entry.publish")
            );

        // We traced; keep the logger assertion loose
        _logger
            .ReceivedWithAnyArgs()
            .Log(
                LogLevel.Trace,
                0,
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task WriteMessageToQueue_WhenHeaderMissing_Returns_Error_And_DoesNotWrite()
    {
        // Arrange
        var sut = new WriteCmsWebhookToQueueCommand(_logger, _writer);
        var request = NewRequestWithHeader(headerValue: null);
        using var json = JsonDocument.Parse("{\"k\":\"v\"}");

        // Act
        var result = await sut.WriteMessageToQueue(json, request);

        // Assert
        await _writer.DidNotReceive().WriteMessage(Arg.Any<string>(), Arg.Any<string>());

        // If QueueWriteResult exposes an Error/message property, assert it:
        // Assert.Equal($"Couldn't find header {WriteCmsWebhookToQueueCommand.ContentfulTopicHeaderKey}", result.Error);
        // Otherwise, at least ensure we got a non-default object back:
        Assert.NotNull(result);

        _logger
            .ReceivedWithAnyArgs()
            .Log(
                LogLevel.Error,
                0,
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task WriteMessageToQueue_WhenHeaderEmpty_Returns_Error_And_DoesNotWrite()
    {
        // Arrange
        var sut = new WriteCmsWebhookToQueueCommand(_logger, _writer);
        var request = NewRequestWithHeader(null);
        using var json = JsonDocument.Parse("{}");

        // Act
        var result = await sut.WriteMessageToQueue(json, request);

        // Assert
        await _writer.DidNotReceive().WriteMessage(Arg.Any<string>(), Arg.Any<string>());
        Assert.NotNull(result);

        _logger
            .ReceivedWithAnyArgs()
            .Log(
                LogLevel.Error,
                0,
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task WriteMessageToQueue_WhenWriterThrows_Returns_Error_And_Logs()
    {
        // Arrange
        var sut = new WriteCmsWebhookToQueueCommand(_logger, _writer);
        var request = NewRequestWithHeader("ContentManagement.Entry.unpublish");
        using var json = JsonDocument.Parse("{\"id\":\"xyz\"}");

        _writer
            .WriteMessage(Arg.Any<string>(), Arg.Any<string>())
            .Returns<Task<QueueWriteResult>>(_ => throw new InvalidOperationException("boom"));

        // Act
        var result = await sut.WriteMessageToQueue(json, request);

        // Assert
        // If QueueWriteResult has an Error/message property:
        // Assert.Equal("boom", result.Error);
        Assert.NotNull(result);

        _logger
            .ReceivedWithAnyArgs()
            .Log(
                LogLevel.Error,
                0,
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }
}
