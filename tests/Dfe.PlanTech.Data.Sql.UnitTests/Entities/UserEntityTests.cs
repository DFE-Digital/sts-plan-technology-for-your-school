using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class UserEntityTests
{
    [Fact]
    public void UserEntity_AsDto_PropertiesMapCorrectly()
    {
        // Arrange
        var expectedId = 21;
        var expectedDsiRef = "dsi-abc";
        var expectedDateCreated = new DateTime(2024, 04, 01, 09, 00, 00, DateTimeKind.Utc);
        var expectedDateLastUpdated = new DateTime(2024, 04, 02, 09, 00, 00, DateTimeKind.Utc);

        var entity = new UserEntity
        {
            Id = expectedId,
            DfeSignInRef = expectedDsiRef,
            DateCreated = expectedDateCreated,
            DateLastUpdated = expectedDateLastUpdated,
            Responses = null
        };

        // Act
        SqlUserDto dto = entity.AsDto();

        // Assert
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedDsiRef, dto.DfeSignInRef);
        Assert.Equal(expectedDateCreated, dto.DateCreated);
        Assert.Equal(expectedDateLastUpdated, dto.DateLastUpdated);
        Assert.Null(dto.Responses);
    }

    [Fact]
    public void UserEntity_AsDto_DefaultsDateCreatedToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        var entity = new UserEntity
        {
            Id = 22,
            DfeSignInRef = "dsi-123"
            // DateCreated not set explicitly
        };

        var after = DateTime.UtcNow;

        // Act
        SqlUserDto dto = entity.AsDto();

        // Assert
        Assert.InRange(dto.DateCreated, before, after);
        Assert.Equal(DateTimeKind.Utc, dto.DateCreated.Kind);
    }
}
