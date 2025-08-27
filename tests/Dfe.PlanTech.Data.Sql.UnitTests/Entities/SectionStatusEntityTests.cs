using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class SectionStatusEntityTests
{
    [Fact]
    public void SectionStatusEntity_AsDto_PropertiesMapCorrectly()
    {
        // Arrange
        var expectedSectionId = "Arbitrary string - section id";
        var expectedCompleted = true;
        var expectedLastMaturity = "Arbitrary string - last maturity";
        var expectedDateCreated = new DateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Utc);
        var expectedDateUpdated = new DateTime(2024, 02, 01, 10, 00, 00, DateTimeKind.Utc);
        bool? expectedViewed = true;
        var expectedLastCompletionDate = new DateTime(2024, 03, 01, 10, 00, 00, DateTimeKind.Utc);

        var entity = new SectionStatusEntity
        {
            SectionId = expectedSectionId,
            Completed = expectedCompleted,
            LastMaturity = expectedLastMaturity,
            DateCreated = expectedDateCreated,
            DateUpdated = expectedDateUpdated,
            Viewed = expectedViewed,
            LastCompletionDate = expectedLastCompletionDate
        };

        // Act
        SqlSectionStatusDto dto = entity.AsDto();

        // Assert
        Assert.Equal(expectedSectionId, dto.SectionId);
        Assert.Equal(expectedCompleted, dto.Completed);
        Assert.Equal(expectedLastMaturity, dto.LastMaturity);
        Assert.Equal(expectedDateCreated, dto.DateCreated);
        Assert.Equal(expectedDateUpdated, dto.DateUpdated);
        Assert.Equal(expectedViewed, dto.HasBeenViewed);
        Assert.Equal(expectedLastCompletionDate, dto.LastCompletionDate);
    }
}
