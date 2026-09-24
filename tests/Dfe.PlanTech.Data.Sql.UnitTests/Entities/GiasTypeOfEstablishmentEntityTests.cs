using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class GiasTypeOfEstablishmentEntityTests
{
    [Fact]
    public void GiasGroupMembershipEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedTypeOfEstablishmentCode = 1;
        var expectedTypeOfEstablishmentName = "Establishment type";

        var entity = new GiasTypeOfEstablishmentEntity
        {
            TypeOfEstablishmentCode = expectedTypeOfEstablishmentCode,
            TypeOfEstablishmentName = expectedTypeOfEstablishmentName,
        };

        // Act
        SqlGiasTypeOfEstablishmentDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedTypeOfEstablishmentCode, dto.TypeOfEstablishmentCode);
        Assert.Equal(expectedTypeOfEstablishmentName, dto.TypeOfEstablishmentName);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlGiasTypeOfEstablishmentDto>([
            nameof(SqlGiasTypeOfEstablishmentDto.TypeOfEstablishmentCode),
            nameof(SqlGiasTypeOfEstablishmentDto.TypeOfEstablishmentName),
        ]);
    }
}
