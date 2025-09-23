using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class EstablishmentRecommendationHistoryEntityTests
{
    [Fact]
    public void EstablishmentRecommendationHistoryEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedEstablishmentId = 1;
        var expectedRecommendationId = 2;
        var expectedUserId = 3;
        var expectedMatEstablishmentId = 4;
        var expectedDateCreated = new DateTime(2024, 05, 01, 12, 00, 00, DateTimeKind.Utc);
        var expectedPreviousStatus = "Arbitrary string - previous status";
        var expectedNewStatus = "Arbitrary string - new status";
        var expectedNoteText = "Arbitrary string - note text";

        var entity = new EstablishmentRecommendationHistoryEntity
        {
            Id = 42,
            EstablishmentId = expectedEstablishmentId,
            RecommendationId = expectedRecommendationId,
            UserId = expectedUserId,
            MatEstablishmentId = expectedMatEstablishmentId,
            DateCreated = expectedDateCreated,
            PreviousStatus = expectedPreviousStatus,
            NewStatus = expectedNewStatus,
            NoteText = expectedNoteText
        };

        // Act
        SqlEstablishmentRecommendationHistoryDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedEstablishmentId, dto.EstablishmentId);
        Assert.Equal(expectedRecommendationId, dto.RecommendationId);
        Assert.Equal(expectedUserId, dto.UserId);
        Assert.Equal(expectedMatEstablishmentId, dto.MatEstablishmentId);
        Assert.Equal(expectedDateCreated, dto.DateCreated);
        Assert.Equal(expectedPreviousStatus, dto.PreviousStatus);
        Assert.Equal(expectedNewStatus, dto.NewStatus);
        Assert.Equal(expectedNoteText, dto.NoteText);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlEstablishmentRecommendationHistoryDto>(
            new[]
            {
                nameof(SqlEstablishmentRecommendationHistoryDto.EstablishmentId),
                nameof(SqlEstablishmentRecommendationHistoryDto.RecommendationId),
                nameof(SqlEstablishmentRecommendationHistoryDto.UserId),
                nameof(SqlEstablishmentRecommendationHistoryDto.MatEstablishmentId),
                nameof(SqlEstablishmentRecommendationHistoryDto.DateCreated),
                nameof(SqlEstablishmentRecommendationHistoryDto.PreviousStatus),
                nameof(SqlEstablishmentRecommendationHistoryDto.NewStatus),
                nameof(SqlEstablishmentRecommendationHistoryDto.NoteText)
            }
        );
    }

    [Fact]
    public void EstablishmentRecommendationHistoryEntity_AsDto_WhenOptionalPropertiesNull_HandlesNullsCorrectly()
    {
        // Arrange
        var entity = new EstablishmentRecommendationHistoryEntity
        {
            Id = 1,
            EstablishmentId = 1,
            RecommendationId = 2,
            UserId = 3,
            MatEstablishmentId = null, // Optional
            DateCreated = DateTime.UtcNow,
            PreviousStatus = null, // Optional
            NewStatus = "Arbitrary string - new status",
            NoteText = null // Optional
        };

        // Act
        SqlEstablishmentRecommendationHistoryDto dto = entity.AsDto();

        // Assert
        Assert.Null(dto.MatEstablishmentId);
        Assert.Null(dto.PreviousStatus);
        Assert.Null(dto.NoteText);
        Assert.Equal("Arbitrary string - new status", dto.NewStatus);
    }

    [Fact]
    public void EstablishmentRecommendationHistoryEntity_AsDto_WhenDateCreatedNotProvided_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        var entity = new EstablishmentRecommendationHistoryEntity
        {
            Id = 1,
            EstablishmentId = 1,
            RecommendationId = 2,
            UserId = 3,
            NewStatus = "Arbitrary string - new status"
            // DateCreated is not set explicitly
        };

        var after = DateTime.UtcNow;

        // Act
        SqlEstablishmentRecommendationHistoryDto dto = entity.AsDto();

        // Assert
        Assert.InRange(dto.DateCreated, before, after);
        Assert.Equal(DateTimeKind.Utc, dto.DateCreated.Kind);
    }
}
