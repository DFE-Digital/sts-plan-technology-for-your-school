using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class EstablishmentGroupEntityTests
{
    [Fact]
    public void EstablishmentGroupEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedId = 7;
        var expectedUid = "Arbitrary string - uid";
        var expectedGroupName = "Arbitrary string - group name";
        var expectedGroupType = "Arbitrary string - group type";
        var expectedGroupStatus = "Arbitrary string - group status";

        var entity = new EstablishmentGroupEntity
        {
            Id = expectedId,
            Uid = expectedUid,
            GroupName = expectedGroupName,
            GroupType = expectedGroupType,
            GroupStatus = expectedGroupStatus,
        };

        // Act
        SqlEstablishmentGroupDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedUid, dto.Uid);
        Assert.Equal(expectedGroupName, dto.GroupName);
        Assert.Equal(expectedGroupType, dto.GroupType);
        Assert.Equal(expectedGroupStatus, dto.GroupStatus);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlEstablishmentGroupDto>(
            new[]
            {
                nameof(SqlEstablishmentGroupDto.Id),
                nameof(SqlEstablishmentGroupDto.Uid),
                nameof(SqlEstablishmentGroupDto.GroupName),
                nameof(SqlEstablishmentGroupDto.GroupType),
                nameof(SqlEstablishmentGroupDto.GroupStatus),
            }
        );
    }
}
