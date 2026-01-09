using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class SectionStatusEntityTests
{
    [Fact]
    public void SectionStatusEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedSectionId = "Arbitrary string - section id";
        var expectedLastMaturity = "Arbitrary string - last maturity";
        var expectedDateCreated = new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Utc);
        var expectedDateUpdated = new DateTime(2024, 02, 01, 10, 00, 00, DateTimeKind.Utc);
        bool? expectedViewed = true;
        var expectedLastCompletionDate = new DateTime(2024, 03, 01, 10, 00, 00, DateTimeKind.Utc);
        var expectedStatus = SubmissionStatus.None; // default value

        var entity = new SectionStatusEntity
        {
            SectionId = expectedSectionId,
            LastMaturity = expectedLastMaturity,
            DateCreated = expectedDateCreated,
            DateUpdated = expectedDateUpdated,
            Viewed = expectedViewed,
            LastCompletionDate = expectedLastCompletionDate,
        };

        // Act
        SqlSectionStatusDto dto = entity.AsDto();

        // Assert - properties explicitly set by AsDto()
        Assert.Equal(expectedSectionId, dto.SectionId);
        Assert.Equal(expectedLastMaturity, dto.LastMaturity);
        Assert.Equal(expectedDateCreated, dto.DateCreated);
        Assert.Equal(expectedDateUpdated, dto.DateUpdated);
        Assert.Equal(expectedViewed, dto.HasBeenViewed);
        Assert.Equal(expectedLastCompletionDate, dto.LastCompletionDate);

        // Assert - properties not explicitly set by AsDto():
        Assert.Equal(expectedStatus, dto.Status);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlSectionStatusDto>(
            new[]
            {
            nameof(SqlSectionStatusDto.SectionId),
            nameof(SqlSectionStatusDto.LastMaturity),
            nameof(SqlSectionStatusDto.DateCreated),
            nameof(SqlSectionStatusDto.DateUpdated),
            nameof(SqlSectionStatusDto.HasBeenViewed),
            nameof(SqlSectionStatusDto.LastCompletionDate),
            },
            new[]
            {
                // Synthetic property, not directly mapped from database entity
                nameof(SqlSectionStatusDto.Status),
            }
        );
    }
}
