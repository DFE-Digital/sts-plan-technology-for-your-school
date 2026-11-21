using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class GetSelectASelfAssessmentTests : GroupsControllerTestBase
{
    [Fact]
    public async Task GetSelectASelfAssessment_WhenSchoolsAreSelected_ClearsSelectedSchoolsFromSession()
    {
        // Arrange
        _session.SetSelectedSchools([
            new SelectedEstablishmentDataModel
            {
                EstablishmentId = 11,
                Urn = "100011",
                Name = "Oakfield Primary",
            },
        ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.GetSelectASelfAssessment();

        // Assert
        Assert.Empty(_session.GetSelectedSchools());
    }

    [Fact]
    public async Task GetSelectASelfAssessment_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var expectedResult = new OkResult();
        var systemUnderTest = CreateSystemUnderTest();
        _groupsViewBuilder
            .RouteToSelectASelfAssessmentViewModelAsync(systemUnderTest)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.GetSelectASelfAssessment();

        // Assert
        Assert.Same(expectedResult, result);
    }
}
