using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class UserSettingsEntityTests
{
    [Fact]
    public void UserSetttingsEntity_AsDto_WhenEntityHasValues_PropertiesMappedCorrectly()
    {
        // Arrange
        var expectedUserId = 21;
        var expectedSortOrder = RecommendationSortOrder.LastUpdated;

        var entity = new UserSettingsEntity
        {
            UserId = expectedUserId,
            SortOrder = expectedSortOrder.ToString(),
        };

        // Act
        SqlUserSettingsDto dto = entity.AsDto();

        // Assert - properties explicitly set by `AsDto()`
        Assert.Equal(expectedUserId, dto.UserId);
        Assert.Equal(expectedSortOrder, dto.SortOrder);

        // Assert - ensure all DTO properties are accounted for
        DtoPropertyCoverageAssert.AssertAllPropertiesAccountedFor<SqlUserSettingsDto>([
            nameof(SqlUserSettingsDto.UserId),
            nameof(SqlUserSettingsDto.SortOrder),
        ]);
    }
}
