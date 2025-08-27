using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class EstablishmentEntityTests
{
    [Fact]
    public void EstablishmentEntity_AsDto_PropertiesMapCorrectly()
    {
        // Arrange
        var expectedId = 10;
        var expectedRef = "Arbitrary string - establishment ref";
        var expectedType = "Arbitrary string - establishment type";
        var expectedOrgName = "Arbitrary string - organisation name";
        var expectedGroupUid = "Arbitrary string - group uid";
        var expectedDateCreated = new DateTime(2024, 05, 01, 12, 00, 00, DateTimeKind.Utc);
        var expectedDateLastUpdated = new DateTime(2024, 05, 10, 12, 00, 00, DateTimeKind.Utc);

        var entity = new EstablishmentEntity
        {
            Id = expectedId,
            EstablishmentRef = expectedRef,
            EstablishmentType = expectedType,
            OrgName = expectedOrgName,
            GroupUid = expectedGroupUid,
            DateCreated = expectedDateCreated,
            DateLastUpdated = expectedDateLastUpdated
        };

        // Act
        SqlEstablishmentDto dto = entity.AsDto();

        // Assert
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedRef, dto.EstablishmentRef);
        Assert.Equal(expectedType, dto.EstablishmentType);
        Assert.Equal(expectedOrgName, dto.OrgName);
        Assert.Equal(expectedGroupUid, dto.GroupUid);
        Assert.Equal(expectedDateCreated, dto.DateCreated);
        Assert.Equal(expectedDateLastUpdated, dto.DateLastUpdated);
    }

    [Fact]
    public void EstablishmentEntity_AsDto_DefaultsDateCreatedToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        var entity = new EstablishmentEntity
        {
            Id = 1,
            EstablishmentRef = "Arbitrary string - establishment ref",
            EstablishmentType = "Arbitrary string - establishment type",
            OrgName = "Arbitrary string - organisation name"
            // DateCreated is not set explicitly
        };

        var after = DateTime.UtcNow;

        // Act
        SqlEstablishmentDto dto = entity.AsDto();

        // Assert
        Assert.InRange(dto.DateCreated, before, after);
        Assert.Equal(DateTimeKind.Utc, dto.DateCreated.Kind);
    }
}
