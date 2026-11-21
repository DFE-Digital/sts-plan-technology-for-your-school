using System.Text.Json;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.PagesViewBuilderTests;

public class BuildNotifyShareResultsViewModelTests : PagesViewBuilderTestBase
{
    [Fact]
    public void BuildNotifyShareResultsViewModel_WhenTempDataHasNoShareResults_ReturnsNull()
    {
        // Arrange
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = systemUnderTest.BuildNotifyShareResultsViewModel(controller);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void BuildNotifyShareResultsViewModel_WhenTempDataHasShareResults_ReturnsDeserialisedViewModel()
    {
        // Arrange
        var shareResults = new NotifyShareResultsViewModel
        {
            ActionModel = new ActionViewModel(
                "GetByRoute",
                "PagesController",
                "Back to networking",
                new Dictionary<string, object> { { "route", "networking" } }
            ),
            SendResults =
            [
                new NotifySendResult { Recipient = "recipient@example.com", Errors = [] },
                new NotifySendResult { Recipient = "failed@example.com", Errors = ["send failed"] },
            ],
        };
        var controller = new TestController();
        controller.TempData[StatePassingMechanismConstants.NotifyShareResults] =
            JsonSerializer.Serialize(shareResults);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = systemUnderTest.BuildNotifyShareResultsViewModel(controller);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GetByRoute", result.ActionModel.ActionName);
        Assert.Equal("Pages", result.ActionModel.ControllerName);
        Assert.Equal("Back to networking", result.ActionModel.LinkText);
        Assert.Collection(
            result.SendResults,
            sendResult =>
            {
                Assert.Equal("recipient@example.com", sendResult.Recipient);
                Assert.Empty(sendResult.Errors);
            },
            sendResult =>
            {
                Assert.Equal("failed@example.com", sendResult.Recipient);
                Assert.Equal(["send failed"], sendResult.Errors);
            }
        );
    }
}
