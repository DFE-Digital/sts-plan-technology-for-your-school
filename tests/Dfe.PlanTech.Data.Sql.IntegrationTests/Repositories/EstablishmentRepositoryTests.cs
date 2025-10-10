using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class EstablishmentRepositoryTests : DatabaseIntegrationTestBase
{
    private EstablishmentRepository _repository = null!;

    public EstablishmentRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new EstablishmentRepository(DbContext);
    }

    [Fact]
    public async Task EstablishmentRepository_CreateEstablishmentFromModelAsync_WhenGivenValidModel_ThenCreatesAndReturnsEntity()
    {
        // Arrange
        var model = new OrganisationModel
        {
            Urn = "TEST123",
            Name = "Test School",
            Type = new IdWithNameModel { Name = "Academy" },
            GroupUid = "group-123"
        };

        // Act
        var result = await _repository.CreateEstablishmentFromModelAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("TEST123", result.EstablishmentRef);
        Assert.Equal("Test School", result.OrgName);
        Assert.Equal("Academy", result.EstablishmentType);
        Assert.Equal("group-123", result.GroupUid);

        // Verify it was saved to database
        var saved = await DbContext.Establishments.FindAsync(result.Id);
        Assert.NotNull(saved);
        Assert.Equal(model.Reference, saved!.EstablishmentRef);
    }

    [Fact]
    public async Task EstablishmentRepository_CreateEstablishmentFromModelAsync_WhenTypeIsNull_ThenHandlesNullType()
    {
        // Arrange
        var model = new OrganisationModel
        {
            Urn = "TEST456",
            Name = "Test School 2",
            Type = null
        };

        // Act
        var result = await _repository.CreateEstablishmentFromModelAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEST456", result.EstablishmentRef);
        Assert.Equal("Test School 2", result.OrgName);
        Assert.Null(result.EstablishmentType);
    }

    [Fact]
    public async Task EstablishmentRepository_CreateEstablishmentFromModelAsync_WhenModelIsNull_ThenThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _repository.CreateEstablishmentFromModelAsync(null!));
    }

    [Fact]
    public async Task EstablishmentRepository_GetEstablishmentByReferenceAsync_WhenEstablishmentExists_ThenReturnsExistingEstablishment()
    {
        // Arrange
        var establishment = new EstablishmentEntity
        {
            EstablishmentRef = "EXIST123",
            OrgName = "Existing School"
        };
        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetEstablishmentByReferenceAsync("EXIST123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EXIST123", result!.EstablishmentRef);
        Assert.Equal("Existing School", result.OrgName);
    }

    [Fact]
    public async Task EstablishmentRepository_GetEstablishmentByReferenceAsync_WhenEstablishmentDoesNotExist_ThenReturnsNull()
    {
        // Act
        var result = await _repository.GetEstablishmentByReferenceAsync("NONEXISTENT");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EstablishmentRepository_GetEstablishmentsByReferencesAsync_WhenReferencesMatch_ThenReturnsMatchingEstablishments()
    {
        // Arrange
        var establishment1 = new EstablishmentEntity { EstablishmentRef = "REF001", OrgName = "School 1" };
        var establishment2 = new EstablishmentEntity { EstablishmentRef = "REF002", OrgName = "School 2" };
        var establishment3 = new EstablishmentEntity { EstablishmentRef = "REF003", OrgName = "School 3" };

        DbContext.Establishments.AddRange(establishment1, establishment2, establishment3);
        await DbContext.SaveChangesAsync();

        var references = new[] { "REF001", "REF003", "NONEXISTENT" };

        // Act
        var result = await _repository.GetEstablishmentsByReferencesAsync(references);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.EstablishmentRef == "REF001");
        Assert.Contains(result, e => e.EstablishmentRef == "REF003");
        Assert.DoesNotContain(result, e => e.EstablishmentRef == "REF002");
    }

    [Fact]
    public async Task EstablishmentRepository_GetEstablishmentsByReferencesAsync_WhenNoReferencesMatch_ThenReturnsEmptyCollection()
    {
        // Arrange
        var establishment = new EstablishmentEntity { EstablishmentRef = "EXISTING", OrgName = "School" };
        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync();

        var references = new[] { "NONEXISTENT1", "NONEXISTENT2" };

        // Act
        var result = await _repository.GetEstablishmentsByReferencesAsync(references);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task EstablishmentRepository_GetEstablishmentsByAsync_WhenPredicateMatches_ThenFiltersEstablishmentsByPredicate()
    {
        // Arrange
        var establishment1 = new EstablishmentEntity { EstablishmentRef = "ACADEMY1", OrgName = "Academy School", EstablishmentType = "Academy" };
        var establishment2 = new EstablishmentEntity { EstablishmentRef = "PRIMARY1", OrgName = "Primary School", EstablishmentType = "Primary" };
        var establishment3 = new EstablishmentEntity { EstablishmentRef = "ACADEMY2", OrgName = "Another Academy", EstablishmentType = "Academy" };

        DbContext.Establishments.AddRange(establishment1, establishment2, establishment3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetEstablishmentsByAsync(e => e.EstablishmentType == "Academy");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("Academy", e.EstablishmentType));
    }

    [Fact]
    public async Task EstablishmentRepository_GetEstablishmentsByAsync_WhenPredicateDoesNotMatch_ThenReturnsEmptyResult()
    {
        // Arrange
        var establishment = new EstablishmentEntity { EstablishmentRef = "EXISTING", OrgName = "School" };
        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetEstablishmentsByAsync(e => e.EstablishmentRef == "NONEXISTENT");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
