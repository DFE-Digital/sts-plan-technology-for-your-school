using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Tests.Shared.Builders.Models;

namespace Dfe.PlanTech.Application.UnitTests.Providers.CurrentUserProviderTests;

public class OrganisationCategoryTests : CurrentUserProviderTestBase
{
    [Theory]
    [InlineData(DsiConstants.EstablishmentCategoryId, true)]
    [InlineData(DsiConstants.MatOrganisationCategoryId, false)]
    [InlineData(DsiConstants.LocalAuthorityCategoryId, false)]
    public void IsSchool_WhenCategoryIsGiven_ReturnsWhetherItIsAnEstablishment(
        string categoryId,
        bool expectedResult
    )
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(categoryId, "Category")
            .Build();
        var systemUnderTest = CreateSystemUnderTest(organisation);

        // Act
        var result = systemUnderTest.IsSchool;

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(DsiConstants.MatOrganisationCategoryId, true)]
    [InlineData(DsiConstants.SatOrganisationCategoryId, false)]
    [InlineData(DsiConstants.EstablishmentCategoryId, false)]
    public void IsMat_WhenCategoryIsGiven_ReturnsWhetherItIsAMultiAcademyTrust(
        string categoryId,
        bool expectedResult
    )
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(categoryId, "Category")
            .Build();
        var systemUnderTest = CreateSystemUnderTest(organisation);

        // Act
        var result = systemUnderTest.IsMat;

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(DsiConstants.SatOrganisationCategoryId, true)]
    [InlineData(DsiConstants.SSatOrganisationCategoryId, true)]
    [InlineData(DsiConstants.MatOrganisationCategoryId, false)]
    [InlineData(DsiConstants.EstablishmentCategoryId, false)]
    public void IsSatOrSSat_WhenCategoryIsGiven_ReturnsWhetherItIsASingleAcademyTrust(
        string categoryId,
        bool expectedResult
    )
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(categoryId, "Category")
            .Build();
        var systemUnderTest = CreateSystemUnderTest(organisation);

        // Act
        var result = systemUnderTest.IsSatOrSSat;

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(DsiConstants.MatOrganisationCategoryId, true)]
    [InlineData(DsiConstants.SatOrganisationCategoryId, true)]
    [InlineData(DsiConstants.SSatOrganisationCategoryId, true)]
    [InlineData(DsiConstants.EstablishmentCategoryId, false)]
    public void IsTrust_WhenCategoryIsGiven_ReturnsWhetherItIsAnyKindOfTrust(
        string categoryId,
        bool expectedResult
    )
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(categoryId, "Category")
            .Build();
        var systemUnderTest = CreateSystemUnderTest(organisation);

        // Act
        var result = systemUnderTest.IsTrust;

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(DsiConstants.MatOrganisationCategoryId, true)]
    [InlineData(DsiConstants.SatOrganisationCategoryId, false)] // Single-academy trusts have exactly one school
    [InlineData(DsiConstants.EstablishmentCategoryId, false)]
    public void CanSelectSchools_WhenCategoryIsGiven_ReturnsWhetherItIsAGroupThatSelectsSchools(
        string categoryId,
        bool expectedResult
    )
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(categoryId, "Category")
            .Build();
        var systemUnderTest = CreateSystemUnderTest(organisation);

        // Act
        var result = systemUnderTest.CanSelectSchools;

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void CategoryFlags_WhenThereIsNoOrganisation_AreAllFalse()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        Assert.False(systemUnderTest.IsSchool);
        Assert.False(systemUnderTest.IsMat);
        Assert.False(systemUnderTest.IsSatOrSSat);
        Assert.False(systemUnderTest.IsTrust);
        Assert.False(systemUnderTest.CanSelectSchools);
    }
}
