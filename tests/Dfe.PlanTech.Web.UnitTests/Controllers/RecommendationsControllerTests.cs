using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class RecommendationsControllerTests
{
    private readonly RecommendationsController _recommendationsController;

    public RecommendationsControllerTests()
    {
        Mock<ILogger<RecommendationsController>> loggerMock = new Mock<ILogger<RecommendationsController>>();
        _recommendationsController = new RecommendationsController(loggerMock.Object);
    }

    [Fact]
    public async Task RecommendationsPage_Displays_BackButton()
    {
        var result = await _recommendationsController.GetRecommendationPage();
        Assert.IsType<ViewResult>(result);
    }
}