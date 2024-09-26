using System.Text.Json;
using Dfe.PlanTech.Application.Content.Commands;
using Dfe.PlanTech.Application.Queues.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Content.Commands;

public class WriteCmsWebhookToQueueCommandTests
{
    private const string JsonString = "{}";
    private const string CmsEvent = "test-header";

    private readonly IQueueWriter _queueWriter = Substitute.For<IQueueWriter>();

    private readonly ILogger<WriteCmsWebhookToQueueCommand> _logger =
        Substitute.For<ILogger<WriteCmsWebhookToQueueCommand>>();

    private readonly HttpContext _httpContext = new DefaultHttpContext();

    private readonly IWriteCmsWebhookToQueueCommand _command;


    public WriteCmsWebhookToQueueCommandTests()
    {
        _command = new WriteCmsWebhookToQueueCommand(_queueWriter, _logger);
    }

    [Fact]
    public async Task WriteMessageToQueue_Should_Error_When_CmsEvent_NotFound()
    {
        var body = CreateWebhookBody();

        var result = await _command.WriteMessageToQueue(body!, _httpContext.Request);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task WriteMessageToQueue_Should_Write_Valid_Message()
    {
        var cmsEvent = AddCmsEventHeader();

        var body = CreateWebhookBody();

        var result = await _command.WriteMessageToQueue(body, _httpContext.Request);
        Assert.Null(result);

        await _queueWriter.Received(1).WriteMessage(JsonString, cmsEvent);
    }

    [Fact]
    public async Task WriteMessageToQueue_Should_Handle_Exception()
    {
        AddCmsEventHeader();
        var body = CreateWebhookBody();
        var exception = new Exception("Thrown exception");
        _queueWriter.WriteMessage(Arg.Any<string>(), Arg.Any<string>()).ThrowsAsync(exception);

        var result = await _command.WriteMessageToQueue(body, _httpContext.Request);

        Assert.False(result.Success);
        Assert.Equal(exception.Message, result.ErrorMessage);
        var loggedMessages = _logger.GetMatchingReceivedMessages("Failed to save message to queue", LogLevel.Error);

        Assert.Single(loggedMessages);
    }
    private static JsonDocument CreateWebhookBody() => JsonSerializer.Deserialize<JsonDocument>(JsonString)!;

    private string AddCmsEventHeader()
    {
        _httpContext.Request.Headers.Append(WriteCmsWebhookToQueueCommand.ContentfulTopicHeaderKey, CmsEvent);
        return CmsEvent;
    }

}
