using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class CookiesControllerTests
{
    private readonly CookiesController _cookiesController;

    public CookiesControllerTests()
    {
        Mock<ILogger<CookiesController>> loggerMock = new Mock<ILogger<CookiesController>>();
        _cookiesController = new CookiesController(loggerMock.Object);
    }

    [Fact]
    public async Task CookiesPageDisplaysBackButton()
    {
        var result = await _cookiesController.GetCookiesPage();
        Assert.IsType<ViewResult>(result);
    }
}