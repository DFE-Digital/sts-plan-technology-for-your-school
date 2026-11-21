using Dfe.PlanTech.Web.Controllers;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.RecommendationsControllerTests;

public class ConstructorTests : RecommendationsControllerTestBase
{
    // RecommendationsController does not guard its logger, so ConstructorGuardAssert cannot be used here.
    [Fact]
    public void Constructor_WhenViewBuilderIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new RecommendationsController(_logger, null!)
        );
        Assert.Equal("recommendationsViewBuilder", exception.ParamName);
    }
}
