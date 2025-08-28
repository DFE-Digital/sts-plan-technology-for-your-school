using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class EstablishmentLinkEntityTests
{
    [Fact]
    public void EstablishmentLinkEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
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

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedGroupUid, dto.GroupUid);
        Assert.Equal(expectedEstablishmentName, dto.EstablishmentName);
        Assert.Equal(expectedUrn, dto.Urn);

        // Assert - properties not explicitly set by `AsDto()`:
        Assert.Null(dto.SectionStatuses); // TODO: Remove this property - not used/not required
        Assert.Null(dto.CompletedSectionsCount); // Assigned dynamically within `Dfe.PlanTech.Application.Services.EstablishmentService.GetEstablishmentLinksWithSubmissionStatusesAndCounts`

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlEstablishmentLinkDto>(
            new[]
            {
                nameof(SqlEstablishmentLinkDto.Id),
                nameof(SqlEstablishmentLinkDto.GroupUid),
                nameof(SqlEstablishmentLinkDto.EstablishmentName),
                nameof(SqlEstablishmentLinkDto.Urn),
                nameof(SqlEstablishmentLinkDto.SectionStatuses)
            },
            new[]
            {
                // Assigned dynamically within `Dfe.PlanTech.Application.Services.EstablishmentService.GetEstablishmentLinksWithSubmissionStatusesAndCounts`
                nameof(SqlEstablishmentLinkDto.CompletedSectionsCount)
            }
        );
    }
}
