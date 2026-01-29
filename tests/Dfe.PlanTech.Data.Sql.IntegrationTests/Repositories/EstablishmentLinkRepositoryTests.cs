using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class EstablishmentLinkRepositoryTests : DatabaseIntegrationTestBase
{
    private EstablishmentLinkRepository _repository = null!;

    public EstablishmentLinkRepositoryTests(DatabaseFixture fixture)
        : base(fixture) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new EstablishmentLinkRepository(DbContext);
    }

    [Fact]
    public async Task EstablishmentLinkRepository_GetGroupEstablishmentsByEstablishmentIdAsync_WhenEstablishmentIdIsValid_ThenReturnsLinksForEstablishment()
    {
        // Arrange
        var groupUid = "test-group-123";
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Test School",
            GroupUid = groupUid,
        };

        var group = new EstablishmentGroupEntity { Uid = groupUid, GroupName = "Test Group" };

        var link1 = new EstablishmentLinkEntity
        {
            GroupUid = groupUid,
            Urn = "URN001",
            EstablishmentName = "Linked School 1",
        };

        var link2 = new EstablishmentLinkEntity
        {
            GroupUid = groupUid,
            Urn = "URN002",
            EstablishmentName = "Linked School 2",
        };

        DbContext.Establishments.Add(establishment);
        DbContext.EstablishmentGroups.Add(group);
        DbContext.EstablishmentLinks.AddRange(link1, link2);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetGroupEstablishmentsByEstablishmentIdAsync(
            establishment.Id
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, l => l.Urn == "URN001");
        Assert.Contains(result, l => l.Urn == "URN002");
    }

    [Fact]
    public async Task EstablishmentLinkRepository_GetGroupEstablishmentsByEstablishmentIdAsync_WhenEstablishmentIdDoesNotExist_ThenReturnsEmptyCollection()
    {
        // Act
        var result = await _repository.GetGroupEstablishmentsByEstablishmentIdAsync(99999);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task EstablishmentLinkRepository_GetGroupEstablishmentsByAsync_WhenPredicateMatches_ThenFiltersEstablishmentsByPredicate()
    {
        // Arrange
        var groupUid1 = "group-1";
        var groupUid2 = "group-2";

        var establishment1 = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "School 1",
            GroupUid = groupUid1,
        };

        var establishment2 = new EstablishmentEntity
        {
            EstablishmentRef = "EST002",
            OrgName = "School 2",
            GroupUid = groupUid2,
        };

        var group1 = new EstablishmentGroupEntity { Uid = groupUid1, GroupName = "Group 1" };
        var group2 = new EstablishmentGroupEntity { Uid = groupUid2, GroupName = "Group 2" };

        var link1 = new EstablishmentLinkEntity
        {
            GroupUid = groupUid1,
            Urn = "URN001",
            EstablishmentName = "Link 1",
        };
        var link2 = new EstablishmentLinkEntity
        {
            GroupUid = groupUid2,
            Urn = "URN002",
            EstablishmentName = "Link 2",
        };

        DbContext.Establishments.AddRange(establishment1, establishment2);
        DbContext.EstablishmentGroups.AddRange(group1, group2);
        DbContext.EstablishmentLinks.AddRange(link1, link2);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetGroupEstablishmentsByAsync(e =>
            e.EstablishmentRef == "EST001"
        );

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("URN001", result[0].Urn);
    }

    [Fact]
    public async Task EstablishmentLinkRepository_GetGroupEstablishmentsByAsync_WhenEstablishmentHasNoGroup_ThenHandlesEstablishmentWithoutGroup()
    {
        // Arrange
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EST001",
            OrgName = "Orphaned School",
            GroupUid = null, // No group
        };

        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetGroupEstablishmentsByAsync(e => e.Id == establishment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task EstablishmentLinkRepository_RecordGroupSelection_WhenModelIsValid_ThenInsertsRowAndReturnsId()
    {
        // Arrange
        var model = new UserGroupSelectionModel
        {
            UserId = 1,
            UserEstablishmentId = 10,
            SelectedEstablishmentId = 99,
            SelectedEstablishmentName = "Selected School"
        };

        // Act
        var id = await _repository.RecordGroupSelection(model);

        // Assert
        Assert.True(id > 0);

        var saved = await DbContext.GroupReadActivities.FindAsync(id);
        Assert.NotNull(saved);
        Assert.Equal(1, saved!.UserId);
        Assert.Equal(10, saved.UserEstablishmentId);
        Assert.Equal(99, saved.SelectedEstablishmentId);
        Assert.Equal("Selected School", saved.SelectedEstablishmentName);
    }

    [Fact]
    public async Task EstablishmentLinkRepository_RecordGroupSelection_WhenSelectedEstablishmentNameIsNull_ThenStoresEmptyString()
    {
        // Arrange
        var model = new UserGroupSelectionModel
        {
            UserId = 2,
            UserEstablishmentId = 20,
            SelectedEstablishmentId = 199,
            SelectedEstablishmentName = null
        };

        // Act
        var id = await _repository.RecordGroupSelection(model);

        // Assert
        Assert.True(id > 0);

        var saved = await DbContext.GroupReadActivities.FindAsync(id);
        Assert.NotNull(saved);
        Assert.Equal(string.Empty, saved!.SelectedEstablishmentName);
    }

    [Fact]
    public async Task EstablishmentLinkRepository_RecordGroupSelection_WhenModelIsNull_ThenThrows()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.RecordGroupSelection(null!));
    }

    [Fact]
    public async Task EstablishmentLinkRepository_RecordGroupSelection_WhenCalledMultipleTimes_ThenCreatesMultipleRows()
    {
        // Arrange
        var model1 = new UserGroupSelectionModel
        {
            UserId = 3,
            UserEstablishmentId = 30,
            SelectedEstablishmentId = 300,
            SelectedEstablishmentName = "School A"
        };

        var model2 = new UserGroupSelectionModel
        {
            UserId = 3,
            UserEstablishmentId = 30,
            SelectedEstablishmentId = 301,
            SelectedEstablishmentName = "School B"
        };

        // Act
        var id1 = await _repository.RecordGroupSelection(model1);
        var id2 = await _repository.RecordGroupSelection(model2);

        // Assert
        Assert.True(id1 > 0);
        Assert.True(id2 > 0);
        Assert.NotEqual(id1, id2);

        var saved1 = await DbContext.GroupReadActivities.FindAsync(id1);
        var saved2 = await DbContext.GroupReadActivities.FindAsync(id2);

        Assert.NotNull(saved1);
        Assert.NotNull(saved2);
        Assert.Equal("School A", saved1!.SelectedEstablishmentName);
        Assert.Equal("School B", saved2!.SelectedEstablishmentName);
    }

}
