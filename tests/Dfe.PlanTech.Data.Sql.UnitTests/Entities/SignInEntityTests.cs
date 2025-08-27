using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class SignInEntityTests
{
    [Fact]
    public void SignInEntity_AsDto_PropertiesMapCorrectly()
    {
        // Arrange
        var expectedId = 31;
        var expectedUserId = 41;
        int? expectedEstablishmentId = 51;
        var expectedSignInDateTime = new DateTime(2024, 07, 01, 14, 30, 00, DateTimeKind.Utc);

        var user = new UserEntity
        {
            Id = expectedUserId,
            DfeSignInRef = "Arbitrary string - dsi ref",
            DateCreated = new DateTime(2024, 01, 01, 00, 00, 00, DateTimeKind.Utc),
            Responses = null
        };

        var entity = new SignInEntity
        {
            Id = expectedId,
            UserId = expectedUserId,
            EstablishmentId = expectedEstablishmentId,
            SignInDateTime = expectedSignInDateTime,
            User = user
        };

        // Act
        SqlSignInDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedUserId, dto.UserId);
        Assert.Equal(expectedEstablishmentId, dto.EstablishmentId);
        Assert.Equal(expectedSignInDateTime, dto.SignInDateTime);
        Assert.NotNull(dto.User);
        Assert.Equal(expectedUserId, dto.User.Id);
        Assert.Equal("Arbitrary string - dsi ref", dto.User.DfeSignInRef);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlSignInDto>(
            new[]
            {
                nameof(SqlSignInDto.Id),
                nameof(SqlSignInDto.UserId),
                nameof(SqlSignInDto.EstablishmentId),
                nameof(SqlSignInDto.SignInDateTime),
                nameof(SqlSignInDto.User)
            }
        );
    }
}
