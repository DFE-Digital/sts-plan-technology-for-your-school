using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class AnswerEntityTests
{
    // Validate that the `AsDto()` method correctly maps properties
    [Fact]
    public void AnswerEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedId = 1;
        var expectedAnswerText = "Arbitrary string - answer text";
        var expectedContentfulRef = "Arbitrary string - contentful ref";
        var expectedDateCreated = new DateTime(2023, 01, 01, 12, 00, 00, DateTimeKind.Utc);
        var expectedUserActionId = Guid.NewGuid();

        AnswerEntity answerEntity = new()
        {
            Id = expectedId,
            AnswerText = expectedAnswerText,
            ContentfulRef = expectedContentfulRef,
            DateCreated = expectedDateCreated,
            UserActionId = expectedUserActionId
        };

        // Act
        SqlAnswerDto sqlAnswerDto = answerEntity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedId, sqlAnswerDto.Id);
        Assert.Equal(expectedAnswerText, sqlAnswerDto.AnswerText);
        Assert.Equal(expectedContentfulRef, sqlAnswerDto.ContentfulSysId);
        Assert.Equal(expectedDateCreated, sqlAnswerDto.DateCreated);
        Assert.Equal(expectedUserActionId, sqlAnswerDto.UserActionId);


        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlAnswerDto>(
            new[]
            {
                nameof(SqlAnswerDto.Id),
                nameof(SqlAnswerDto.AnswerText),
                nameof(SqlAnswerDto.ContentfulSysId),
                nameof(SqlAnswerDto.DateCreated),
                nameof(SqlAnswerDto.UserActionId)
            }
        );
    }

    [Fact]
    public void AnswerEntity_AsDto_WhenDateCreatedNotProvided_DefaultsToUtcNow()
    {
        // Arrange
        var before = System.DateTime.UtcNow;
        AnswerEntity answerEntity = new();
        var after = System.DateTime.UtcNow;

        // Act
        SqlAnswerDto dto = answerEntity.AsDto();

        // Assert: DateCreated is set to a value between 'before' and 'after', and is UTC
        Assert.InRange(dto.DateCreated, before, after);
        Assert.Equal(System.DateTimeKind.Utc, dto.DateCreated.Kind);
    }
}
