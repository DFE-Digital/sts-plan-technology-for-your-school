using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class RecommendationEntityTests
{
    [Fact]
    public void RecommendationEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedId = 1;
        var expectedRecommendationText = "Arbitrary string - recommendation text";
        var expectedContentfulRef = "Arbitrary string - contentful ref";
        var expectedDateCreated = new DateTime(2024, 05, 01, 12, 00, 00, DateTimeKind.Utc);
        var expectedQuestionContentfulRef = "1234";
        var expectedArchived = true;

        var entity = new RecommendationEntity
        {
            Id = expectedId,
            RecommendationText = expectedRecommendationText,
            ContentfulRef = expectedContentfulRef,
            DateCreated = expectedDateCreated,
            QuestionContentfulRef = expectedQuestionContentfulRef,
            Archived = expectedArchived
        };

        // Act
        SqlRecommendationDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedRecommendationText, dto.RecommendationText);
        Assert.Equal(expectedContentfulRef, dto.ContentfulSysId);
        Assert.Equal(expectedDateCreated, dto.DateCreated);
        Assert.Equal(expectedQuestionContentfulRef, dto.QuestionContentfulRef);
        Assert.Equal(expectedArchived, dto.Archived);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlRecommendationDto>(
            new[]
            {
                nameof(SqlRecommendationDto.Id),
                nameof(SqlRecommendationDto.RecommendationText),
                nameof(SqlRecommendationDto.ContentfulSysId),
                nameof(SqlRecommendationDto.DateCreated),
                nameof(SqlRecommendationDto.QuestionContentfulRef),
                nameof(SqlRecommendationDto.Archived)
            }
        );
    }

    [Fact]
    public void RecommendationEntity_AsDto_WhenOptionalPropertiesNull_HandlesNullsCorrectly()
    {
        // Arrange
        var questionEntity = new QuestionEntity
        {
            Id = 1,
            QuestionText = "Arbitrary string - question text",
            ContentfulRef = "Arbitrary string - question contentful ref",
            DateCreated = DateTime.UtcNow
        };

        var entity = new RecommendationEntity
        {
            Id = 1,
            RecommendationText = null, // Optional
            ContentfulRef = "Arbitrary string - contentful ref",
            DateCreated = DateTime.UtcNow,
            QuestionContentfulRef = "1234",
            Archived = false
        };

        // Act
        SqlRecommendationDto dto = entity.AsDto();

        // Assert
        Assert.Null(dto.RecommendationText);
        Assert.Equal("Arbitrary string - contentful ref", dto.ContentfulSysId);
        Assert.False(dto.Archived);
        Assert.NotNull(dto.QuestionContentfulRef);
    }

    [Fact]
    public void RecommendationEntity_AsDto_WhenDateCreatedNotProvided_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        var questionEntity = new QuestionEntity
        {
            Id = 1,
            QuestionText = "Arbitrary string - question text",
            ContentfulRef = "Arbitrary string - question contentful ref",
            DateCreated = DateTime.UtcNow
        };

        var entity = new RecommendationEntity
        {
            Id = 1,
            RecommendationText = "Arbitrary string - recommendation text",
            ContentfulRef = "Arbitrary string - contentful ref",
            QuestionContentfulRef = "1234"
            // DateCreated is not set explicitly
        };

        var after = DateTime.UtcNow;

        // Act
        SqlRecommendationDto dto = entity.AsDto();

        // Assert
        Assert.InRange(dto.DateCreated, before, after);
        Assert.Equal(DateTimeKind.Utc, dto.DateCreated.Kind);
    }

    [Fact]
    public void RecommendationEntity_AsDto_WhenArchivedNotProvided_DefaultsToFalse()
    {
        // Arrange
        var questionEntity = new QuestionEntity
        {
            Id = 1,
            QuestionText = "Arbitrary string - question text",
            ContentfulRef = "Arbitrary string - question contentful ref",
            DateCreated = DateTime.UtcNow
        };

        var entity = new RecommendationEntity
        {
            Id = 1,
            RecommendationText = "Arbitrary string - recommendation text",
            ContentfulRef = "Arbitrary string - contentful ref",
            DateCreated = DateTime.UtcNow,
            QuestionContentfulRef = "1234"
            // Archived is not set explicitly
        };

        // Act
        SqlRecommendationDto dto = entity.AsDto();

        // Assert
        Assert.False(dto.Archived);
    }
}
