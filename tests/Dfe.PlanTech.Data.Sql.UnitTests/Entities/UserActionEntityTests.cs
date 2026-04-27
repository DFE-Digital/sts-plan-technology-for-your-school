using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class UserActionEntityTests
{
    [Fact]
    public void UserActionEntity_WhenCreatedWithValues_PropertiesSetCorrectly()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var expectedUserId = 101;
        var expectedEstablishmentId = 202;
        var expectedMatEstablishmentId = 303;
        var expectedRequestedUrl = "/test-url";
        var expectedDateCreated = new DateTime(2026, 04, 27, 12, 00, 00, DateTimeKind.Utc);

        // Act
        UserActionEntity userActionEntity = new()
        {
            Id = expectedId,
            UserId = expectedUserId,
            EstablishmentId = expectedEstablishmentId,
            MatEstablishmentId = expectedMatEstablishmentId,
            RequestedUrl = expectedRequestedUrl,
            DateCreated = expectedDateCreated,
        };

        // Assert
        Assert.Equal(expectedId, userActionEntity.Id);
        Assert.Equal(expectedUserId, userActionEntity.UserId);
        Assert.Equal(expectedEstablishmentId, userActionEntity.EstablishmentId);
        Assert.Equal(expectedMatEstablishmentId, userActionEntity.MatEstablishmentId);
        Assert.Equal(expectedRequestedUrl, userActionEntity.RequestedUrl);
        Assert.Equal(expectedDateCreated, userActionEntity.DateCreated);
    }

    [Fact]
    public void UserActionEntity_WhenOptionalValuesNotProvided_OptionalPropertiesAreNull()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var expectedUserId = 101;
        var expectedRequestedUrl = "/test-url";

        // Act
        UserActionEntity userActionEntity = new()
        {
            Id = expectedId,
            UserId = expectedUserId,
            RequestedUrl = expectedRequestedUrl,
        };

        // Assert
        Assert.Equal(expectedId, userActionEntity.Id);
        Assert.Equal(expectedUserId, userActionEntity.UserId);
        Assert.Null(userActionEntity.EstablishmentId);
        Assert.Null(userActionEntity.MatEstablishmentId);
        Assert.Equal(expectedRequestedUrl, userActionEntity.RequestedUrl);
    }
}
