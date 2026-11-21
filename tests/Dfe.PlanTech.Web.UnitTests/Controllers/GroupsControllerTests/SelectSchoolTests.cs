using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.GroupsControllerTests;

public class SelectSchoolTests : GroupsControllerTestBase
{
    private const string SchoolUrn = "100011";
    private const string SchoolName = "Oakfield Primary";

    private readonly SelectedEstablishmentDataModel _focusedSchool = new()
    {
        EstablishmentId = 11,
        Urn = SchoolUrn,
        Name = SchoolName,
    };

    [Fact]
    public async Task SelectSchool_WhenCalled_SetsFocusedEstablishment()
    {
        // Arrange
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(_focusedSchool);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SelectSchool(SchoolUrn, SchoolName);

        // Assert
        await _currentUserProvider.Received(1).SetFocusedEstablishmentAsync(SchoolUrn, SchoolName);
    }

    [Fact]
    public async Task SelectSchool_WhenCalled_RecordsGroupSelectionForFocusedEstablishment()
    {
        // Arrange
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(_focusedSchool);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SelectSchool(SchoolUrn, SchoolName);

        // Assert
        await _groupsViewBuilder.Received(1).RecordGroupSelectionAsync(11, SchoolUrn, SchoolName);
    }

    [Fact]
    public async Task SelectSchool_WhenCalled_RedirectsToHomePage()
    {
        // Arrange
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(_focusedSchool);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SelectSchool(SchoolUrn, SchoolName);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(UrlConstants.HomePage, redirect.Url);
    }
}
