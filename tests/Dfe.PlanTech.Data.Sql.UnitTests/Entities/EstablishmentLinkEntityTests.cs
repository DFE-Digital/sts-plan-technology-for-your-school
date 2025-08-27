using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class EstablishmentLinkEntityTests
{
    [Fact]
    public void EstablishmentLinkEntity_AsDto_PropertiesMapCorrectly()
    {
        // Arrange
        var expectedId = 11;
        var expectedGroupUid = "Arbitrary string - group uid";
        var expectedEstablishmentName = "Arbitrary string - establishment name";
        var expectedUrn = "Arbitrary string - urn";

        var entity = new EstablishmentLinkEntity
        {
            Id = expectedId,
            GroupUid = expectedGroupUid,
            EstablishmentName = expectedEstablishmentName,
            Urn = expectedUrn
        };

        // Act
        SqlEstablishmentLinkDto dto = entity.AsDto();

        // Assert
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedGroupUid, dto.GroupUid);
        Assert.Equal(expectedEstablishmentName, dto.EstablishmentName);
        Assert.Equal(expectedUrn, dto.Urn);
    }
}
