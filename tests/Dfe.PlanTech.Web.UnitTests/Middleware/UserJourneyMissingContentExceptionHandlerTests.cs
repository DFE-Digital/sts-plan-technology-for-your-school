using Dfe.PlanTech.Domain.Exceptions;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class MockController : Controller
{

}

public class UserJourneyMissingContentExceptionHandlerTests
{
    private readonly ILogger<UserJourneyMissingContentExceptionHandler> _logger = Substitute.For<ILogger<UserJourneyMissingContentExceptionHandler>>();
    private readonly IDeleteCurrentSubmissionCommand _deleteCurrentSubmissionCommand = Substitute.For<IDeleteCurrentSubmissionCommand>();
    private readonly IConfiguration _configuration;
    private readonly Controller _controller;

    private const string ErrorMessage = "An error occurred. Please try again.";

    public UserJourneyMissingContentExceptionHandlerTests()
    {
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());

        var inMemorySettings = new Dictionary<string, string?> { { UserJourneyMissingContentExceptionHandler.ErrorMessageConfigKey, ErrorMessage } };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _controller = new MockController()
        {
            TempData = tempData
        };
    }

    [Fact]
    public async Task Handle_LogsError_DeletesSubmission_StoresErrorMessageAndRedirects()
    {
        var section = new Section()
        {
            Name = "Section Name",
            Sys = new()
            {
                Id = "SectionId"
            }
        };

        var expectedLogMessage = $"Handling errored user journey for section {section.Name}";

        var errorHandler = new UserJourneyMissingContentExceptionHandler(_logger, _deleteCurrentSubmissionCommand, _configuration);

        var exception = new UserJourneyMissingContentException("Error with content", section);

        var result = await errorHandler.Handle(_controller, exception, CancellationToken.None);

        TestLoggedMessages(expectedLogMessage, LogLevel.Error, 1);

        await _deleteCurrentSubmissionCommand.Received(1).DeleteCurrentSubmission(section, Arg.Is<CancellationToken>(c => !c.IsCancellationRequested));

        Assert.Equal(ErrorMessage, _controller.TempData["SubtopicError"]);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectResult = result as RedirectToActionResult;
        Assert.NotNull(redirectResult);
        Assert.Equal(PagesController.GetPageByRouteAction, redirectResult.ActionName);
        Assert.Equal(PagesController.ControllerName, redirectResult.ControllerName);
        Assert.Equal("self-assessment", redirectResult.RouteValues?["route"]);
    }

    private void TestLoggedMessages(string message, LogLevel logLevel, int receivedCount)
    {
        var receivedCalls = _logger.ReceivedCalls().Select(call =>
        {
            var args = call.GetArguments();
            var message = args[2]?.ToString();
            var logLevel = Enum.Parse<LogLevel>(args[0]?.ToString() ?? "");
            return new
            {
                logLevel,
                message
            };
        }).ToArray();

        var matchingCallCount = receivedCalls.Where(args => args.message == message && args.logLevel == logLevel).Count();
        if (receivedCalls.Length != receivedCount)
        {
            var actualReceivedCalls = string.Join("\n", receivedCalls.Select(call => $"[{call.logLevel}]Message: {call.message}"));
            Assert.Fail($"Expected {receivedCount} logging messages to match but found {receivedCalls.Length} matching calls. Received calls: \n" + actualReceivedCalls);
        }
    }
}
