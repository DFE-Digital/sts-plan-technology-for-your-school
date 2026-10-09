using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class GiasGroupMembershipEntityTests
{
    [Fact]
    public void GiasGroupMembershipEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedId = 1;
        var expectedUrn = 2;
        var expectedGroupUid = 3;
        var expectedSyncedAt = new DateTime(2026, 8, 24, 9, 0, 0);

        var entity = new GiasGroupMembershipEntity
        {
            Id = expectedId,
            Urn = expectedUrn,
            GroupUid = expectedGroupUid,
            SyncedAt = expectedSyncedAt,
        };

        // Act
        SqlGiasGroupMembershipDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedUrn, dto.Urn);
        Assert.Equal(expectedGroupUid, dto.GroupUid);
        Assert.Equal(expectedSyncedAt, dto.SyncedAt);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlGiasGroupMembershipDto>([
            nameof(SqlGiasGroupMembershipDto.Id),
            nameof(SqlGiasGroupMembershipDto.Urn),
            nameof(SqlGiasGroupMembershipDto.GroupUid),
            nameof(SqlGiasGroupMembershipDto.SyncedAt),
        ]);
    }
}
