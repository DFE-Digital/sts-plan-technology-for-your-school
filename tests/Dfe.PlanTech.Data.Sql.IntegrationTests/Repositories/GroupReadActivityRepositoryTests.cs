using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class GroupReadActivityRepositoryTests(DatabaseFixture fixture)
    : DatabaseIntegrationTestBase(fixture)
{
    private GroupReadActivityRepository CreateSystemUnderTest() => new(DbContext);

    [Fact]
    public async Task RecordGroupSelectionAsync_WhenSelectionIsValid_SavesActivityAndReturnsId()
    {
        // Arrange
        var user = new UserEntityBuilder().WithDfeSignInRef("dsi-user-42").Build();
        var group = new EstablishmentEntityBuilder()
            .WithEstablishmentRef("group-reference-301")
            .WithOrgName("Northfield Academy Trust")
            .Build();
        var school = new EstablishmentEntityBuilder()
            .WithEstablishmentRef("100701")
            .WithOrgName("Northfield Primary")
            .Build();
        DbContext.Users.Add(user);
        DbContext.Establishments.AddRange(group, school);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var selection = new UserGroupSelectionModel
        {
            UserId = user.Id,
            UserEstablishmentId = group.Id,
            SelectedEstablishmentId = school.Id,
            SelectedEstablishmentName = school.OrgName,
        };
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RecordGroupSelectionAsync(selection);

        // Assert
        DbContext.ChangeTracker.Clear();
        var savedActivity = await DbContext.GroupReadActivities.SingleAsync(
            activity => activity.Id == result,
            TestContext.Current.CancellationToken
        );
        Assert.True(result > 0);
        Assert.Equal(user.Id, savedActivity.UserId);
        Assert.Equal(group.Id, savedActivity.UserEstablishmentId);
        Assert.Equal(school.Id, savedActivity.SelectedEstablishmentId);
        Assert.Equal("Northfield Primary", savedActivity.SelectedEstablishmentName);
    }
}
