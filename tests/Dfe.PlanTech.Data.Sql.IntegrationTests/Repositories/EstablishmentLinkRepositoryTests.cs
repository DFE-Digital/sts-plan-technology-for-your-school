using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class EstablishmentLinkRepositoryTests : DatabaseIntegrationTestBase
{
    private EstablishmentRepository _establishmentRepository = null!;
    private EstablishmentLinkRepository _establishmentLinkRepository = null!;
    private UserRepository _userRepository = null!;

    public EstablishmentLinkRepositoryTests(DatabaseFixture fixture)
        : base(fixture) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _establishmentRepository = new EstablishmentRepository(DbContext);
        _establishmentLinkRepository = new EstablishmentLinkRepository(DbContext);
        _userRepository = new UserRepository(DbContext);
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
        var result =
            await _establishmentLinkRepository.GetGroupEstablishmentsByEstablishmentIdAsync(
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
        var result =
            await _establishmentLinkRepository.GetGroupEstablishmentsByEstablishmentIdAsync(99999);

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
        var result = await _establishmentLinkRepository.GetGroupEstablishmentsByAsync(e =>
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
        var result = await _establishmentLinkRepository.GetGroupEstablishmentsByAsync(e =>
            e.Id == establishment.Id
        );

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task EstablishmentLinkRepository_RecordGroupSelection_WhenModelIsValid_ThenInsertsRowAndReturnsId()
    {
        // Arrange
        var matModel = new EstablishmentModel
        {
            DistrictAdministrativeCode = "district-admin-code-1",
            LegacyId = "legacy-id-1",
            Name = "Test MAT",
            Sid = "sid-1",
            Urn = "urn-1",
        };

        var schoolModel = new EstablishmentModel
        {
            DistrictAdministrativeCode = "district-admin-code-2",
            LegacyId = "legacy-id-2",
            Name = "Selected School",
            Sid = "sid-2",
            Urn = "urn-2",
        };

        var mat = await _establishmentRepository.CreateEstablishmentFromModelAsync(matModel);
        var school = await _establishmentRepository.CreateEstablishmentFromModelAsync(schoolModel);
        var user = await _userRepository.CreateUserBySignInRefAsync("user-1-ref");

        var model = new UserGroupSelectionModel
        {
            UserId = user.Id,
            UserEstablishmentId = mat.Id,
            SelectedEstablishmentId = school.Id,
            SelectedEstablishmentName = "Selected School",
        };

        // Act
        var id = await _establishmentLinkRepository.RecordGroupSelection(model);

        // Assert
        Assert.True(id > 0);

        var saved = await DbContext.GroupReadActivities.FindAsync(id);
        Assert.NotNull(saved);
        Assert.Equal(schoolModel.Name, saved.SelectedEstablishmentName);
    }

    [Fact]
    public async Task EstablishmentLinkRepository_RecordGroupSelection_WhenSelectedEstablishmentNameIsNull_ThenStoresEmptyString()
    {
        // Arrange
        var matModel = new EstablishmentModel
        {
            DistrictAdministrativeCode = "district-admin-code-1",
            LegacyId = "legacy-id-1",
            Name = "Test MAT",
            Sid = "sid-1",
            Urn = "urn-1",
        };

        var schoolModel = new EstablishmentModel
        {
            DistrictAdministrativeCode = "district-admin-code-2",
            LegacyId = "legacy-id-2",
            Name = "Selected School",
            Sid = "sid-2",
            Urn = "urn-2",
        };

        var mat = await _establishmentRepository.CreateEstablishmentFromModelAsync(matModel);
        var school = await _establishmentRepository.CreateEstablishmentFromModelAsync(schoolModel);
        var user = await _userRepository.CreateUserBySignInRefAsync("user-2-ref");

        var model = new UserGroupSelectionModel
        {
            UserId = user.Id,
            UserEstablishmentId = mat.Id,
            SelectedEstablishmentId = school.Id,
            SelectedEstablishmentName = null,
        };

        // Act
        var id = await _establishmentLinkRepository.RecordGroupSelection(model);

        // Assert
        Assert.True(id > 0);

        var saved = await DbContext.GroupReadActivities.FindAsync(id);
        Assert.NotNull(saved);
        Assert.Equal(string.Empty, saved!.SelectedEstablishmentName);
    }

    [Fact]
    public async Task EstablishmentLinkRepository_RecordGroupSelection_WhenModelIsNull_ThenThrows()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _establishmentLinkRepository.RecordGroupSelection(null!)
        );
    }

    [Fact]
    public async Task EstablishmentLinkRepository_RecordGroupSelection_WhenCalledMultipleTimes_ThenCreatesMultipleRows()
    {
        // Arrange
        var matEstablishmentModel = new EstablishmentModel
        {
            DistrictAdministrativeCode = "district-admin-code-1",
            LegacyId = "legacy-id-1",
            Name = "Test MAT",
            Sid = "sid-1",
            Urn = "urn-1",
        };

        var schoolEstablishmentModel1 = new EstablishmentModel
        {
            DistrictAdministrativeCode = "district-admin-code-2",
            LegacyId = "legacy-id-2",
            Name = "School A",
            Sid = "sid-2",
            Urn = "urn-2",
        };

        var schoolEstablishmentModel2 = new EstablishmentModel
        {
            DistrictAdministrativeCode = "district-admin-code-3",
            LegacyId = "legacy-id-3",
            Name = "School B",
            Sid = "sid-3",
            Urn = "urn-3",
        };

        var mat = await _establishmentRepository.CreateEstablishmentFromModelAsync(
            matEstablishmentModel
        );
        var school1 = await _establishmentRepository.CreateEstablishmentFromModelAsync(
            schoolEstablishmentModel1
        );
        var school2 = await _establishmentRepository.CreateEstablishmentFromModelAsync(
            schoolEstablishmentModel2
        );
        var user = await _userRepository.CreateUserBySignInRefAsync("user-3-ref");

        var model1 = new UserGroupSelectionModel
        {
            UserId = user.Id,
            UserEstablishmentId = mat.Id,
            SelectedEstablishmentId = school1.Id,
            SelectedEstablishmentName = school1.OrgName,
        };

        var model2 = new UserGroupSelectionModel
        {
            UserId = user.Id,
            UserEstablishmentId = mat.Id,
            SelectedEstablishmentId = school2.Id,
            SelectedEstablishmentName = school2.OrgName,
        };

        // Act
        var id1 = await _establishmentLinkRepository.RecordGroupSelection(model1);
        var id2 = await _establishmentLinkRepository.RecordGroupSelection(model2);

        // Assert
        Assert.True(id1 > 0);
        Assert.True(id2 > 0);
        Assert.NotEqual(id1, id2);

        var saved1 = await DbContext.GroupReadActivities.FindAsync(id1);
        var saved2 = await DbContext.GroupReadActivities.FindAsync(id2);

        Assert.NotNull(saved1);
        Assert.NotNull(saved2);
        Assert.Equal(schoolEstablishmentModel1.Name, saved1!.SelectedEstablishmentName);
        Assert.Equal(schoolEstablishmentModel2.Name, saved2!.SelectedEstablishmentName);
    }
}
