using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class GetSelectASchoolViewTests : GroupsControllerTestBase
{
    [Fact]
    public async Task GetSelectASchoolView_WhenCalled_ClearsSelectedSchools()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.GetSelectASchoolView();

        // Assert
        _currentUserProvider.Received(1).ClearSelectedSchools();
    }

    [Fact]
    public async Task GetSelectASchoolView_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var expectedResult = new OkResult();
        var systemUnderTest = CreateSystemUnderTest();
        _groupsViewBuilder
            .RouteToSelectASchoolViewModelAsync(systemUnderTest)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.GetSelectASchoolView();

        // Assert
        Assert.Same(expectedResult, result);
    }
}
