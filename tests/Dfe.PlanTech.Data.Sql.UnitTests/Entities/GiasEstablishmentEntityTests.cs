using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class GiasEstablishmentEntityTests
{
    [Fact]
    public void GiasEstablishmentGroupEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedUrn = 1;
        var expectedUprn = 2;
        var expectedEstablishmentNumber = 3;
        var expectedEstablishmentName = "Establishment name";
        var expectedEstablishmentStatusCode = 4;
        var expectedLocalAuthorityCode = 5;
        var expectedPhaseCode = 6;
        var expectedTypeOfEstablishmentCode = 7;
        var expectedUkprn = "UKPRN123";
        var expectedSyncedAt = new DateTime(2026, 8, 24, 9, 0, 0);

        var entity = new GiasEstablishmentEntity
        {
            Urn = expectedUrn,
            Uprn = expectedUprn,
            EstablishmentNumber = expectedEstablishmentNumber,
            EstablishmentName = expectedEstablishmentName,
            EstablishmentStatusCode = expectedEstablishmentStatusCode,
            LocalAuthorityCode = expectedLocalAuthorityCode,
            PhaseCode = expectedPhaseCode,
            TypeOfEstablishmentCode = expectedTypeOfEstablishmentCode,
            Ukprn = expectedUkprn,
            SyncedAt = expectedSyncedAt,
        };

        // Act
        SqlGiasEstablishmentDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedUrn, dto.Urn);
        Assert.Equal(expectedUprn, dto.Uprn);
        Assert.Equal(expectedEstablishmentNumber, dto.EstablishmentNumber);
        Assert.Equal(expectedEstablishmentName, dto.EstablishmentName);
        Assert.Equal(expectedEstablishmentStatusCode, dto.EstablishmentStatusCode);
        Assert.Equal(expectedLocalAuthorityCode, dto.LocalAuthorityCode);
        Assert.Equal(expectedPhaseCode, dto.PhaseCode);
        Assert.Equal(expectedTypeOfEstablishmentCode, dto.TypeOfEstablishmentCode);
        Assert.Equal(expectedUkprn, dto.Ukprn);
        Assert.Equal(expectedSyncedAt, dto.SyncedAt);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlGiasEstablishmentDto>([
            nameof(SqlGiasEstablishmentDto.Urn),
            nameof(SqlGiasEstablishmentDto.Uprn),
            nameof(SqlGiasEstablishmentDto.EstablishmentNumber),
            nameof(SqlGiasEstablishmentDto.EstablishmentName),
            nameof(SqlGiasEstablishmentDto.EstablishmentStatusCode),
            nameof(SqlGiasEstablishmentDto.LocalAuthorityCode),
            nameof(SqlGiasEstablishmentDto.PhaseCode),
            nameof(SqlGiasEstablishmentDto.TypeOfEstablishmentCode),
            nameof(SqlGiasEstablishmentDto.Ukprn),
            nameof(SqlGiasEstablishmentDto.SyncedAt),
        ]);
    }

    [Fact]
    public void EstablishmentRecommendationHistoryEntity_AsDto_WhenOptionalPropertiesNull_HandlesNullsCorrectly()
    {
        // Arrange
        var entity = new GiasEstablishmentEntity
        {
            Urn = 1,
            Uprn = null, // Optional
            EstablishmentNumber = null, // Optional
            EstablishmentName = "Establishment name",
            EstablishmentStatusCode = 4,
            LocalAuthorityCode = 5,
            PhaseCode = 6,
            TypeOfEstablishmentCode = 7,
            Ukprn = null, // Optional
            SyncedAt = new DateTime(2026, 8, 24, 9, 0, 0),
        };

        // Act
        SqlGiasEstablishmentDto dto = entity.AsDto();

        // Assert
        Assert.Null(dto.Uprn);
        Assert.Null(dto.EstablishmentNumber);
        Assert.Null(dto.Ukprn);
    }
}
