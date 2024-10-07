using System.Text.Json;
using Dfe.PlanTech.Application.Content.Commands;
using Dfe.PlanTech.Domain.Queues.Models;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class CmsControllerTests
{
    private readonly IWriteCmsWebhookToQueueCommand _writeCmsWebhook = Substitute.For<IWriteCmsWebhookToQueueCommand>();
    private readonly ILogger<CmsController> _logger = Substitute.For<ILogger<CmsController>>();
    private readonly CmsController _controller;

    public CmsControllerTests()
    {
        _controller = new CmsController(_logger);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task WebhookPayload_Should_Call_Command_And_Return_Ok_When_Success()
    {
        var json = "{}";
        var deserialised = JsonSerializer.Deserialize<JsonDocument>(json);
        var expectedResult = new QueueWriteResult(true);
        _writeCmsWebhook.WriteMessageToQueue(deserialised!, Arg.Any<HttpRequest>()).Returns(expectedResult);

        var result = await _controller.WebhookPayload(deserialised!, _writeCmsWebhook);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task WebhookPayload_Should_Call_Command_And_Return_BadRequest_When_Error()
    {
        var json = "{}";
        var deserialised = JsonSerializer.Deserialize<JsonDocument>(json);
        var expectedResult = new QueueWriteResult("Expected error message");

        _writeCmsWebhook.WriteMessageToQueue(deserialised!, Arg.Any<HttpRequest>()).Returns(expectedResult);

        var result = await _controller.WebhookPayload(deserialised!, _writeCmsWebhook);
        Assert.IsType<BadRequestObjectResult>(result);

        var badRequestResult = (BadRequestObjectResult)result;

        var body = badRequestResult.Value;
        Assert.NotNull(body);
        Assert.IsType<QueueWriteResult>(body);

        var queueWriteResult = (QueueWriteResult)body;
        Assert.NotNull(queueWriteResult);
        Assert.Equal(expectedResult.Success, queueWriteResult.Success);
        Assert.Equal(expectedResult.ErrorMessage, queueWriteResult.ErrorMessage);
    }

    [Fact]
    public async Task WebhookPayload_Should_Handle_Error()
    {
        var json = "{}";
        var deserialised = JsonSerializer.Deserialize<JsonDocument>(json);

        var exceptionMessage = "Exception thrown";

        _writeCmsWebhook.WriteMessageToQueue(deserialised!, Arg.Any<HttpRequest>())
            .Throws(new Exception(exceptionMessage));

        var result = await _controller.WebhookPayload(deserialised!, _writeCmsWebhook);
        Assert.IsType<BadRequestObjectResult>(result);

        var badRequestResult = (BadRequestObjectResult)result;

        var body = badRequestResult.Value;
        Assert.NotNull(body);
        Assert.IsType<string>(body);

        Assert.Equal(exceptionMessage, (string)body);

        var loggedMessages = _logger.ReceivedLogMessages().ToArray();

        Assert.Single(loggedMessages);

        Assert.Contains("An error occured while trying to write the message to the queue:", loggedMessages[0].Message);
    }
}
