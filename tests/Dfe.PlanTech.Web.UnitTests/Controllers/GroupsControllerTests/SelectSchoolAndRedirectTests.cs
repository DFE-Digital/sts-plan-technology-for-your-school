using Dfe.PlanTech.Core.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class SelectSchoolAndRedirectTests : GroupsControllerTestBase
{
    private const int SchoolId = 11;
    private const string SchoolUrn = "100011";
    private const string SchoolName = "Oakfield Primary";

    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    [InlineData("   ", typeof(ArgumentException))]
    public async Task SelectSchoolAndRedirect_WhenCategorySlugIsMissing_ThrowsArgumentException(
        string? categorySlug,
        Type expectedExceptionType
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync(expectedExceptionType, () =>
            systemUnderTest.SelectSchoolAndRedirect(categorySlug!, SchoolId, SchoolUrn, SchoolName)
        );
        await _groupsViewBuilder
            .DidNotReceiveWithAnyArgs()
            .RecordGroupSelectionAsync(default, default!, default!);
    }

    [Fact]
    public async Task SelectSchoolAndRedirect_WhenCalled_RecordsGroupSelection()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SelectSchoolAndRedirect(
            CategorySlug,
            SchoolId,
            SchoolUrn,
            SchoolName
        );

        // Assert
        await _groupsViewBuilder
            .Received(1)
            .RecordGroupSelectionAsync(SchoolId, SchoolUrn, SchoolName);
    }

    [Fact]
    public async Task SelectSchoolAndRedirect_WhenCalled_ReplacesSelectedSchoolsWithPlaceholderEstablishment()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SelectSchoolAndRedirect(
            CategorySlug,
            SchoolId,
            SchoolUrn,
            SchoolName
        );

        // Assert
        _currentUserProvider.Received(1).ClearSelectedSchools();
        await _currentUserProvider
            .Received(1)
            .SetSelectedSchoolsAsync(
                Arg.Is<IEnumerable<SelectedEstablishmentDataModel>>(schools =>
                    schools.Count() == 1
                    && schools.Single().EstablishmentId == -1
                    && schools.Single().Urn == SchoolUrn
                    && schools.Single().Name == SchoolName
                )
            );
    }

    [Fact]
    public async Task SelectSchoolAndRedirect_WhenCalled_RedirectsToMatRecommendationsLanding()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SelectSchoolAndRedirect(
            CategorySlug,
            SchoolId,
            SchoolUrn,
            SchoolName
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetMatRecommendationsLanding", redirect.ActionName);
        Assert.Null(redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues!["categorySlug"]);
    }
}
