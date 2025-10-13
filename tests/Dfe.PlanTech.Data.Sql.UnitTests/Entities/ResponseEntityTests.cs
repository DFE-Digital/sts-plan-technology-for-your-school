using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class ResponseEntityTests
{
    [Fact]
    public void ResponseEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedId = 81;
        var expectedUserId = 91;
        var expectedSubmissionId = 101;
        var expectedQuestionId = 111;
        var expectedAnswerId = 121;
        var expectedMaturity = "Arbitrary string - maturity";
        var expectedDateCreated = new DateTime(2024, 09, 01, 15, 00, 00, DateTimeKind.Utc);
        DateTime? expectedDateLastUpdated = new DateTime(2024, 09, 02, 15, 00, 00, DateTimeKind.Utc);

        var user = new UserEntity
        {
            Id = expectedUserId,
            DfeSignInRef = "Arbitrary string - dsi ref",
            DateCreated = new DateTime(2024, 01, 01, 00, 00, 00, DateTimeKind.Utc),
            Responses = null
        };

        var establishment = new EstablishmentEntity
        {
            Id = 1,
            EstablishmentRef = "Arbitrary string - establishment ref",
            EstablishmentType = "Arbitrary string - establishment type",
            OrgName = "Arbitrary string - organisation name",
            DateCreated = new DateTime(2024, 01, 01, 00, 00, 00, DateTimeKind.Utc)
        };

        var submission = new SubmissionEntity
        {
            Id = expectedSubmissionId,
            EstablishmentId = establishment.Id,
            Establishment = establishment,
            SectionId = "Arbitrary string - section id",
            SectionName = "Arbitrary string - section name",
            DateCreated = new DateTime(2024, 02, 01, 00, 00, 00, DateTimeKind.Utc),
            Responses = new List<ResponseEntity>()
        };

        var question = new QuestionEntity
        {
            Id = expectedQuestionId,
            QuestionText = "Arbitrary string - question text",
            ContentfulRef = "Arbitrary string - question contentful ref"
        };

        var answer = new AnswerEntity
        {
            Id = expectedAnswerId,
            AnswerText = "Arbitrary string - answer text",
            ContentfulRef = "Arbitrary string - answer contentful ref",
            DateCreated = new DateTime(2024, 03, 01, 00, 00, 00, DateTimeKind.Utc)
        };

        var entity = new ResponseEntity
        {
            Id = expectedId,
            UserId = expectedUserId,
            User = user,
            SubmissionId = expectedSubmissionId,
            Submission = submission,
            QuestionId = expectedQuestionId,
            Question = question,
            AnswerId = expectedAnswerId,
            Answer = answer,
            Maturity = expectedMaturity,
            DateCreated = expectedDateCreated,
            DateLastUpdated = expectedDateLastUpdated
        };

        // Act
        SqlResponseDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedUserId, dto.UserId);
        Assert.NotNull(dto.User);
        Assert.Equal(expectedUserId, dto.User!.Id);
        Assert.Equal(expectedSubmissionId, dto.SubmissionId);
        Assert.Equal(expectedQuestionId, dto.QuestionId);
        Assert.NotNull(dto.Question);
        Assert.Equal(expectedQuestionId, dto.Question!.Id);
        Assert.Equal(expectedAnswerId, dto.AnswerId);
        Assert.NotNull(dto.Answer);
        Assert.Equal(expectedAnswerId, dto.Answer!.Id);
        Assert.Equal(expectedMaturity, dto.Maturity);
        Assert.Equal(expectedDateCreated, dto.DateCreated);
        Assert.Equal(expectedDateLastUpdated, dto.DateLastUpdated);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlResponseDto>(
            new[]
            {
                nameof(SqlResponseDto.Id),
                nameof(SqlResponseDto.UserId),
                nameof(SqlResponseDto.User),
                nameof(SqlResponseDto.UserEstablishmentId),
                nameof(SqlResponseDto.UserEstablishment),
                nameof(SqlResponseDto.SubmissionId),
                nameof(SqlResponseDto.Submission),
                nameof(SqlResponseDto.QuestionId),
                nameof(SqlResponseDto.Question),
                nameof(SqlResponseDto.AnswerId),
                nameof(SqlResponseDto.Answer),
                nameof(SqlResponseDto.Maturity),
                nameof(SqlResponseDto.DateCreated),
                nameof(SqlResponseDto.DateLastUpdated)
            }
        );
    }

    [Fact]
    public void ResponseEntity_AsDto_WhenDateCreatedNotProvided_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        var entity = new ResponseEntity
        {
            Id = 201,
            UserId = 1,
            SubmissionId = 2,
            Submission = new SubmissionEntity
            {
                Id = 2,
                EstablishmentId = 3,
                Establishment = new EstablishmentEntity
                {
                    Id = 3,
                    EstablishmentRef = "Arbitrary string - establishment ref",
                    EstablishmentType = "Arbitrary string - establishment type",
                    OrgName = "Arbitrary string - organisation name",
                    DateCreated = new DateTime(2024, 01, 01, 00, 00, 00, DateTimeKind.Utc)
                },
                SectionId = "Arbitrary string - section id",
                SectionName = "Arbitrary string - section name",
                DateCreated = new DateTime(2024, 02, 01, 00, 00, 00, DateTimeKind.Utc),
                Responses = new List<ResponseEntity>()
            },
            QuestionId = 4,
            Question = new QuestionEntity
            {
                Id = 4,
                QuestionText = "Arbitrary string - question text",
                ContentfulRef = "Arbitrary string - question contentful ref"
            },
            AnswerId = 5,
            Answer = new AnswerEntity
            {
                Id = 5,
                AnswerText = "Arbitrary string - answer text",
                ContentfulRef = "Arbitrary string - answer contentful ref"
            },
            Maturity = "Arbitrary string - maturity"
            // DateCreated not set explicitly
        };

        var after = DateTime.UtcNow;

        // Act
        SqlResponseDto dto = entity.AsDto();

        // Assert
        Assert.InRange(dto.DateCreated, before, after);
        Assert.Equal(DateTimeKind.Utc, dto.DateCreated.Kind);
    }
}
