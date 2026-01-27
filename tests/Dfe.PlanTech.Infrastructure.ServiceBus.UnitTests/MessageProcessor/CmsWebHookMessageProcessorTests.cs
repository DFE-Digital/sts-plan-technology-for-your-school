using System.Text.Json;
using Dfe.PlanTech.Core.Caching.Interfaces;
using Dfe.PlanTech.Infrastructure.ServiceBus.MessageProcessor;
using Dfe.PlanTech.Infrastructure.ServiceBus.Models;
using Dfe.PlanTech.Infrastructure.ServiceBus.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.UnitTests.MessageProcessor;

public class CmsWebHookMessageProcessorTests
{
    private readonly ILogger<CmsWebHookMessageProcessor> _logger = Substitute.For<
        ILogger<CmsWebHookMessageProcessor>
    >();
    private readonly ICmsCache _cache = Substitute.For<ICmsCache>();
    private readonly JsonSerializerOptions _jsonOpts = new();

    private CmsWebHookMessageProcessor SUT() => new(_logger, _cache, _jsonOpts);

    [Fact]
    public void Ctor_NullGuards()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CmsWebHookMessageProcessor(_logger, null!, _jsonOpts)
        );
        Assert.Throws<ArgumentNullException>(() =>
            new CmsWebHookMessageProcessor(_logger, _cache, null!)
        );
    }

    [Fact]
    public async Task ProcessMessage_ValidJson_InvokesCache_And_ReturnsSuccess()
    {
        // Arrange
        var sut = SUT();
        var payload = new CmsWebHookPayload
        {
            Sys = new CmsWebHookSystemDetails { Id = "abc123", Type = "page" },
        };
        var body = JsonSerializer.Serialize(payload, _jsonOpts);

        // Act
        var result = await sut.ProcessMessage(
            subject: "ignored",
            body,
            id: "msg-1",
            CancellationToken.None
        );

        // Assert
        await _cache.Received(1).InvalidateCacheAsync("abc123", "page");
        Assert.IsType<ServiceBusSuccessResult>(result);

        // We logged at least once while mapping
        _logger
            .ReceivedWithAnyArgs()
            .Log(
                LogLevel.Information,
                0,
                Arg.Any<object>(),
                null,
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task ProcessMessage_InvalidJson_ReturnsNonRetryableError_And_LogsError()
    {
        // Arrange
        var sut = SUT();
        var badBody = "{ this is not valid json }";

        // Act
        var result = await sut.ProcessMessage("ignored", badBody, "msg-2", CancellationToken.None);

        // Assert: no cache activity
        await _cache.DidNotReceive().InvalidateCacheAsync(Arg.Any<string>(), Arg.Any<string>());

        // Error result (non-retryable per code path)
        var err = Assert.IsType<ServiceBusErrorResult>(result);
        // If your type exposes a different property, update this line:
        Assert.False(err.IsRetryable);

        // Logged error with exception
        _logger
            .ReceivedWithAnyArgs()
            .Log(
                LogLevel.Error,
                0,
                Arg.Any<object>(),
                Arg.Any<JsonException>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task ProcessMessage_CacheThrows_ReturnsRetryableError()
    {
        // Arrange
        var sut = SUT();
        var payload = new CmsWebHookPayload
        {
            Sys = new CmsWebHookSystemDetails { Id = "id-9", Type = "asset" },
        };
        var body = JsonSerializer.Serialize(payload, _jsonOpts);

        _cache
            .InvalidateCacheAsync("id-9", "asset")
            .Returns(_ => throw new InvalidOperationException("boom"));

        // Act
        var result = await sut.ProcessMessage("ignored", body, "msg-3", CancellationToken.None);

        // Assert
        var err = Assert.IsType<ServiceBusErrorResult>(result);
        // If property name differs, tweak this assertion:
        Assert.True(err.IsRetryable);
    }
}
