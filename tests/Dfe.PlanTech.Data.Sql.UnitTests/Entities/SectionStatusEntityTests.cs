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
        var expectedCompleted = true;
        var expectedLastMaturity = "Arbitrary string - last maturity";
        var expectedDateCreated = new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Utc);
        var expectedDateUpdated = new DateTime(2024, 02, 01, 10, 00, 00, DateTimeKind.Utc);
        bool? expectedViewed = true;
        var expectedLastCompletionDate = new DateTime(2024, 03, 01, 10, 00, 00, DateTimeKind.Utc);
        var expectedStatus = SubmissionStatus.None;

        var entity = new SectionStatusEntity
        {
            SectionId = expectedSectionId,
            Completed = expectedCompleted,
            LastMaturity = expectedLastMaturity,
            DateCreated = expectedDateCreated,
            DateUpdated = expectedDateUpdated,
            Viewed = expectedViewed,
            LastCompletionDate = expectedLastCompletionDate,
            // Note: `Status` is not a property of `SectionStatusEntity`, so is not set here
        };

        // Act
        SqlSectionStatusDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedSectionId, dto.SectionId);
        Assert.Equal(expectedCompleted, dto.Completed);
        Assert.Equal(expectedLastMaturity, dto.LastMaturity);
        Assert.Equal(expectedDateCreated, dto.DateCreated);
        Assert.Equal(expectedDateUpdated, dto.DateUpdated);
        Assert.Equal(expectedViewed, dto.HasBeenViewed);
        Assert.Equal(expectedLastCompletionDate, dto.LastCompletionDate);

        // Assert - properties not explicitly set by `AsDto()`:
        // TODO: Convert `Status` to a lambda / autoproperty in `SectionStatusEntity` so it can be set here
        // TODO: `Status` will not be required in near future - leave as is for now
        Assert.Equal(expectedStatus, dto.Status);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlSectionStatusDto>(
            new[]
            {
                nameof(SqlSectionStatusDto.SectionId),
                nameof(SqlSectionStatusDto.Completed),
                nameof(SqlSectionStatusDto.LastMaturity),
                nameof(SqlSectionStatusDto.DateCreated),
                nameof(SqlSectionStatusDto.DateUpdated),
                nameof(SqlSectionStatusDto.HasBeenViewed),
                nameof(SqlSectionStatusDto.LastCompletionDate),
                nameof(SqlSectionStatusDto.Status),
            }
        );
    }
}
