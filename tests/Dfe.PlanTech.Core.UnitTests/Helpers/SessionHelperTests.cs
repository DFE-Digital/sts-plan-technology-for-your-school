using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Tests.Shared.Fakes;

namespace Dfe.PlanTech.Core.UnitTests.Helpers;

public class SessionHelperTests
{
    private readonly FakeSession _session = new();

    [Fact]
    public void GetValue_WhenKeyIsUnrecognised_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _session.GetValue("unknown-key")
        );
        Assert.Equal("Session does not expect that key", exception.Message);
    }

    [Fact]
    public void GetValue_WhenNothingIsStoredForKey_ReturnsNull()
    {
        // Act
        var result = _session.GetValue(SessionConstants.FocusedEstablishmentKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SetValue_WhenKeyIsUnrecognised_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _session.SetValue("unknown-key", "northfield-value")
        );
        Assert.Equal(
            "Session does not expect that key or value is of wrong type for key",
            exception.Message
        );
    }

    [Theory]
    [InlineData(SessionConstants.FocusedEstablishmentKey)]
    [InlineData(SessionConstants.GroupEstablishmentsKey)]
    [InlineData(SessionConstants.SelectedEstablishmentsKey)]
    [InlineData(SessionConstants.UserEstablishmentKey)]
    public void SetValue_WhenValueTypeIsWrongForKey_ThrowsInvalidOperationException(string key)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _session.SetValue(key, "northfield-value")
        );
        Assert.Equal(
            "Session does not expect that key or value is of wrong type for key",
            exception.Message
        );
        Assert.Empty(_session.Keys);
    }

    [Fact]
    public void SetFocusedEstablishment_WhenCalled_StoresEstablishmentDetails()
    {
        // Act
        _session.SetFocusedEstablishment(701, "100701", "Northfield Primary");

        // Assert
        var result = _session.GetFocusedEstablishment();
        Assert.NotNull(result);
        Assert.Equal(701, result.EstablishmentId);
        Assert.Equal("100701", result.Urn);
        Assert.Equal("Northfield Primary", result.Name);
    }

    [Fact]
    public void GetFocusedEstablishment_WhenNothingIsStored_ReturnsNull()
    {
        // Act
        var result = _session.GetFocusedEstablishment();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ClearFocusedEstablishment_WhenEstablishmentIsStored_RemovesIt()
    {
        // Arrange
        _session.SetFocusedEstablishment(701, "100701", "Northfield Primary");

        // Act
        _session.ClearFocusedEstablishment();

        // Assert
        Assert.Null(_session.GetFocusedEstablishment());
    }

    [Fact]
    public void SetUserEstablishment_WhenCalled_StoresEstablishment()
    {
        // Arrange
        var expectedEstablishment = new SqlEstablishmentDtoBuilder()
            .WithId(301)
            .WithEstablishmentRef("100301")
            .WithOrgName("Northfield Academy Trust")
            .WithGroupUid("group-301")
            .Build();

        // Act
        _session.SetUserEstablishment(expectedEstablishment);

        // Assert
        Assert.Equivalent(expectedEstablishment, _session.GetUserEstablishment());
    }

    [Fact]
    public void GetUserEstablishment_WhenNothingIsStored_ReturnsNull()
    {
        // Act
        var result = _session.GetUserEstablishment();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SetGroupSchools_WhenCalled_StoresSchoolsInOrder()
    {
        // Arrange
        var firstSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(701)
            .WithUrn("100701")
            .WithName("Northfield Primary")
            .Build();
        var secondSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(702)
            .WithUrn("100702")
            .WithName("Southfield Primary")
            .Build();

        // Act
        _session.SetGroupSchools([firstSchool, secondSchool]);

        // Assert
        Assert.Equal(["100701", "100702"], _session.GetGroupSchools().Select(s => s.Urn).ToArray());
    }

    [Fact]
    public void GetGroupSchools_WhenNothingIsStored_ReturnsEmptyList()
    {
        // Act
        var result = _session.GetGroupSchools();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ClearGroupSchools_WhenSchoolsAreStored_RemovesThem()
    {
        // Arrange
        _session.SetGroupSchools([new SelectedEstablishmentDataModelBuilder().Build()]);

        // Act
        _session.ClearGroupSchools();

        // Assert
        Assert.Empty(_session.GetGroupSchools());
    }

    [Fact]
    public void SetSelectedEstablishments_WhenGivenSingleSchoolDetails_StoresThatSchoolOnly()
    {
        // Act
        _session.SetSelectedEstablishments(701, "100701", "Northfield Primary");

        // Assert
        var school = Assert.Single(_session.GetSelectedSchools());
        Assert.Equal(701, school.EstablishmentId);
        Assert.Equal("100701", school.Urn);
        Assert.Equal("Northfield Primary", school.Name);
    }

    [Fact]
    public void SetSelectedSchools_WhenGivenValidSchools_StoresAllSchools()
    {
        // Arrange
        var schools = new List<SelectedEstablishmentDataModel>
        {
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(701)
                .WithUrn("100701")
                .Build(),
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(702)
                .WithUrn("100702")
                .Build(),
        };

        // Act
        _session.SetSelectedSchools(schools);

        // Assert
        Assert.Equal(
            ["100701", "100702"],
            _session.GetSelectedSchools().Select(s => s.Urn).ToArray()
        );
    }

    [Theory]
    [InlineData(0, "100701", "Northfield Primary", "Establishment ID cannot be less than 1")] // Boundary: IDs start at 1
    [InlineData(701, "100701", " ", "Name cannot be empty")]
    [InlineData(701, "", "Northfield Primary", "URN cannot be empty")]
    public void SetSelectedSchools_WhenASchoolIsInvalid_ThrowsInvalidDataExceptionAndStoresNothing(
        int establishmentId,
        string urn,
        string name,
        string expectedMessage
    )
    {
        // Arrange
        var invalidSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(establishmentId)
            .WithUrn(urn)
            .WithName(name)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() =>
            _session.SetSelectedSchools([invalidSchool])
        );
        Assert.Equal(expectedMessage, exception.Message);
        Assert.Empty(_session.GetSelectedSchools());
    }

    [Fact]
    public void GetSelectedSchools_WhenNothingIsStored_ReturnsEmptyList()
    {
        // Act
        var result = _session.GetSelectedSchools();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ClearSelectedEstablishments_WhenSchoolsAreStored_RemovesThem()
    {
        // Arrange
        _session.SetSelectedEstablishments(701, "100701", "Northfield Primary");

        // Act
        _session.ClearSelectedEstablishments();

        // Assert
        Assert.Empty(_session.GetSelectedSchools());
    }
}
