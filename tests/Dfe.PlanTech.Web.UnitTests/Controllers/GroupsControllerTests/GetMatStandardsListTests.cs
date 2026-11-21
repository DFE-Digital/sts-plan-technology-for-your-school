using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class GetMatStandardsListTests : GroupsControllerTestBase
{
    [Fact]
    public async Task GetMatStandardsList_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var expectedResult = new OkResult();
        var systemUnderTest = CreateSystemUnderTest();
        _groupsViewBuilder.RouteToMatStandardsListAsync(systemUnderTest).Returns(expectedResult);

        // Act
        var result = await systemUnderTest.GetMatStandardsList();

        // Assert
        Assert.Same(expectedResult, result);
    }
}
