using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Entities;

public class GroupReadActivityEntityTests
{
    [Fact]
    public void GroupReadActivityEntity_AsDto_PropertiesMapCorrectly()
    {
        // Arrange
        var expectedId = 1;
        var expectedUserId = 2;
        var expectedUserEstablishmentId = 3;
        var expectedSelectedEstablishmentId = 4;
        var expectedSelectedEstablishmentName = "Arbitrary string - selected establishment name";
        var expectedDateSelected = new DateTime(2024, 06, 01, 08, 30, 00, DateTimeKind.Utc);

        var entity = new GroupReadActivityEntity
        {
            Id = expectedId,
            UserId = expectedUserId,
            UserEstablishmentId = expectedUserEstablishmentId,
            SelectedEstablishmentId = expectedSelectedEstablishmentId,
            SelectedEstablishmentName = expectedSelectedEstablishmentName,
            DateSelected = expectedDateSelected
        };

        // Act
        SqlGroupReadActivityDto dto = entity.AsDto();

        // Assert
        Assert.Equal(expectedId, dto.Id);
        Assert.Equal(expectedUserId, dto.UserId);
        Assert.Equal(expectedUserEstablishmentId, dto.UserEstablishmentId);
        Assert.Equal(expectedSelectedEstablishmentId, dto.SelectedEstablishmentId);
        Assert.Equal(expectedSelectedEstablishmentName, dto.SelectedEstablishmentName);
        Assert.Equal(expectedDateSelected, dto.DateSelected);
    }
}
