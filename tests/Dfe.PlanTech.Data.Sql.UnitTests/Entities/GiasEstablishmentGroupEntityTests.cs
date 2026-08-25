using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class GiasEstablishmentGroupEntityTests
{
    [Fact]
    public void GiasEstablishmentGroupEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedGroupUid = 1;
        var expectedGroupId = "ID123";
        var expectedGroupName = "Group name";
        var expectedGroupStatusCode = 2;
        var expectedGroupTypeCode = 3;
        var expectedSyncedAt = new DateTime(2026, 8, 24, 9, 0, 0);
        var expectedUkprn = "PRN456";

        var entity = new GiasEstablishmentGroupEntity
        {
            GroupUid = expectedGroupUid,
            GroupId = expectedGroupId,
            GroupName = expectedGroupName,
            GroupStatusCode = expectedGroupStatusCode,
            GroupTypeCode = expectedGroupTypeCode,
            SyncedAt = expectedSyncedAt,
            Ukprn = expectedUkprn,
        };

        // Act
        SqlGiasEstablishmentGroupDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedGroupUid, dto.GroupUid);
        Assert.Equal(expectedGroupId, dto.GroupId);
        Assert.Equal(expectedGroupName, dto.GroupName);
        Assert.Equal(expectedGroupStatusCode, dto.GroupStatusCode);
        Assert.Equal(expectedGroupTypeCode, dto.GroupTypeCode);
        Assert.Equal(expectedSyncedAt, dto.SyncedAt);
        Assert.Equal(expectedUkprn, dto.Ukprn);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlGiasEstablishmentGroupDto>([
            nameof(SqlGiasEstablishmentGroupDto.GroupUid),
            nameof(SqlGiasEstablishmentGroupDto.GroupId),
            nameof(SqlGiasEstablishmentGroupDto.GroupName),
            nameof(SqlGiasEstablishmentGroupDto.GroupStatusCode),
            nameof(SqlGiasEstablishmentGroupDto.GroupTypeCode),
            nameof(SqlGiasEstablishmentGroupDto.SyncedAt),
            nameof(SqlGiasEstablishmentGroupDto.Ukprn),
        ]);
    }

    [Fact]
    public void EstablishmentRecommendationHistoryEntity_AsDto_WhenOptionalPropertiesNull_HandlesNullsCorrectly()
    {
        // Arrange
        var entity = new GiasEstablishmentGroupEntity
        {
            GroupUid = 1,
            GroupId = null, // Optional
            GroupName = "Group name",
            GroupStatusCode = 2,
            GroupTypeCode = 3,
            SyncedAt = new DateTime(2026, 8, 24, 9, 0, 0),
            Ukprn = null, // Optional
        };

        // Act
        SqlGiasEstablishmentGroupDto dto = entity.AsDto();

        // Assert
        Assert.Null(dto.GroupId);
        Assert.Null(dto.Ukprn);
    }
}
