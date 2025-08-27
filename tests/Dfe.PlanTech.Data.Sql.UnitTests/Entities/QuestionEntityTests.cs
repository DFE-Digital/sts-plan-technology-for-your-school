using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class QuestionEntityTests
{
    [Fact]
    public void QuestionEntity_AsDto_PropertiesMapCorrectly()
    {
        // Arrange
        var expectedId = 5;
        var expectedQuestionText = "Arbitrary string - question text";
        var expectedContentfulRef = "Arbitrary string - contentful ref";

        var entity = new QuestionEntity
        {
            Id = expectedId,
            QuestionText = expectedQuestionText,
            ContentfulRef = expectedContentfulRef
        };

        // Act
        SqlQuestionDto dto = entity.AsDto();

        // Assert
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedQuestionText, dto.QuestionText);
        Assert.Equal(expectedContentfulRef, dto.ContentfulSysId);
    }

    [Fact]
    public void QuestionEntity_AsDto_DefaultsDateCreatedToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        var entity = new QuestionEntity
        {
            Id = 6,
            QuestionText = "Arbitrary string - question text",
            ContentfulRef = "Arbitrary string - contentful ref"
        };

        var after = DateTime.UtcNow;

        // Act
        SqlQuestionDto dto = entity.AsDto();

        // Assert
        Assert.InRange(dto.DateCreated, before, after);
        Assert.Equal(DateTimeKind.Utc, dto.DateCreated.Kind);
    }
}
