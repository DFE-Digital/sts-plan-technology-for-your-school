
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class RecommendationsControllerTests
{
    private readonly RecommendationsController _recommendationsController;
    private readonly Mock<IPlanTechDbContext> _planTechDbContextMock;
    private object _calculateMaturityCommandMock;

    public RecommendationsControllerTests()
    {
        Mock<ILogger<RecommendationsController>> loggerMock = new Mock<ILogger<RecommendationsController>>();
        Mock<IUrlHistory> urlHistoryMock = new Mock<IUrlHistory>();
        _recommendationsController = new RecommendationsController(loggerMock.Object, urlHistoryMock.Object);
    }

    [Fact]
    public async Task RecommendationsPage_Displays_BackButton()
    {
        var result = _recommendationsController.GetRecommendationPage();
        Assert.IsType<ViewResult>(result);
    }
}