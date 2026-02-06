using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class SubmissionEntityTests
{
    [Fact]
    public void SubmissionEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedId = 61;
        var expectedEstablishmentId = 71;
        var expectedSectionId = "Arbitrary string - section id";
        var expectedSectionName = "Arbitrary string - section name";
        var expectedMaturity = "Arbitrary string - maturity";
        var expectedDateCreated = new DateTime(2024, 08, 01, 10, 00, 00, DateTimeKind.Utc);
        DateTime? expectedDateLastUpdated = new DateTime(
            2024,
            08,
            02,
            11,
            00,
            00,
            DateTimeKind.Utc
        );
        DateTime? expectedDateCompleted = null;
        var expectedDeleted = false;
        var expectedViewed = true;
        var expectedStatus = SubmissionStatus.InProgress;

        var establishment = new EstablishmentEntity
        {
            Id = expectedEstablishmentId,
            EstablishmentRef = "Arbitrary string - establishment ref",
            EstablishmentType = "Arbitrary string - establishment type",
            OrgName = "Arbitrary string - organisation name",
            DateCreated = new DateTime(2024, 01, 01, 00, 00, 00, DateTimeKind.Utc),
        };

        var entity = new SubmissionEntity
        {
            Id = expectedId,
            EstablishmentId = expectedEstablishmentId,
            Establishment = establishment,
            SectionId = expectedSectionId,
            SectionName = expectedSectionName,
            Maturity = expectedMaturity,
            DateCreated = expectedDateCreated,
            DateLastUpdated = expectedDateLastUpdated,
            DateCompleted = expectedDateCompleted,
            // Note deliberate empty collection here to prevent circular references (a full response has a reference to the submission).
            // TODO: Additional tests to validate the mapping (e.g. to verify no infinite recursion occurs with circular references).
            // TODO: Consider if it is reasonable to redesign this to remove the circular references / risk for infinite recursion when mapping.
            Responses = new List<ResponseEntity>(),
            Deleted = expectedDeleted,
            Viewed = expectedViewed,
            Status = expectedStatus,
        };

        // Act
        SqlSubmissionDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedEstablishmentId, dto.EstablishmentId);
        Assert.NotNull(dto.Establishment);
        Assert.Equal(expectedEstablishmentId, dto.Establishment.Id);
        Assert.Equal(expectedSectionId, dto.SectionId);
        Assert.Equal(expectedSectionName, dto.SectionName);
        Assert.Equal(expectedMaturity, dto.Maturity);
        Assert.Equal(expectedDateCreated, dto.DateCreated);
        Assert.Equal(expectedDateLastUpdated, dto.DateLastUpdated);
        Assert.Equal(expectedDateCompleted, dto.DateCompleted);
        Assert.Empty(dto.Responses);
        Assert.Equal(expectedDeleted, dto.Deleted);
        Assert.Equal(expectedViewed, dto.Viewed);
        Assert.Equal(expectedStatus, dto.Status);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlSubmissionDto>(
            new[]
            {
                nameof(SqlSubmissionDto.Id),
                nameof(SqlSubmissionDto.EstablishmentId),
                nameof(SqlSubmissionDto.Establishment),
                nameof(SqlSubmissionDto.SectionId),
                nameof(SqlSubmissionDto.SectionName),
                nameof(SqlSubmissionDto.Maturity),
                nameof(SqlSubmissionDto.DateCreated),
                nameof(SqlSubmissionDto.DateLastUpdated),
                nameof(SqlSubmissionDto.DateCompleted),
                nameof(SqlSubmissionDto.Responses),
                nameof(SqlSubmissionDto.Deleted),
                nameof(SqlSubmissionDto.Viewed),
                nameof(SqlSubmissionDto.Status),
            }
        );
    }
}
