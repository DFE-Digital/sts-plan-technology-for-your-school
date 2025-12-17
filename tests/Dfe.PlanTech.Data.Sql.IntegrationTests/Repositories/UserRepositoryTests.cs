using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class UserRepositoryTests : DatabaseIntegrationTestBase
{
    private UserRepository _repository = null!;

    public UserRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new UserRepository(DbContext);
    }

    [Fact]
    public async Task UserRepository_CreateUserBySignInRefAsync_WhenGivenValidSignInRef_ThenCreatesAndReturnsUser()
    {
        // Arrange
        var dfeSignInRef = "new-user-123";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var result = await _repository.CreateUserBySignInRefAsync(dfeSignInRef);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(dfeSignInRef, result.DfeSignInRef);
        Assert.True(result.DateCreated >= beforeCreation);
        Assert.True(result.DateCreated <= DateTime.UtcNow);

        // Verify it was saved to database
        var saved = await DbContext.Users.FindAsync(result.Id);
        Assert.NotNull(saved);
        Assert.Equal(dfeSignInRef, saved!.DfeSignInRef);
    }

    [Fact]
    public async Task UserRepository_GetUserBySignInRefAsync_WhenUserExists_ThenReturnsExistingUser()
    {
        // Arrange
        var user = new UserEntity
        {
            DfeSignInRef = "existing-user-456",
            DateCreated = DateTime.UtcNow
        };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserBySignInRefAsync("existing-user-456");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
        Assert.Equal("existing-user-456", result.DfeSignInRef);
    }

    [Fact]
    public async Task UserRepository_GetUserBySignInRefAsync_WhenUserDoesNotExist_ThenReturnsNull()
    {
        // Act
        var result = await _repository.GetUserBySignInRefAsync("nonexistent-user");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UserRepository_GetUserByAsync_WhenPredicateMatches_ThenFiltersAndReturnsUser()
    {
        // Arrange
        var user1 = new UserEntity { DfeSignInRef = "user-alpha", DateCreated = DateTime.UtcNow.AddDays(-5) };
        var user2 = new UserEntity { DfeSignInRef = "user-beta", DateCreated = DateTime.UtcNow.AddDays(-1) };
        var user3 = new UserEntity { DfeSignInRef = "user-gamma", DateCreated = DateTime.UtcNow };

        DbContext.Users.AddRange(user1, user2, user3);
        await DbContext.SaveChangesAsync();

        // Act - Find user created more than 2 days ago
        var result = await _repository.GetUserByAsync(u => u.DateCreated < DateTime.UtcNow.AddDays(-2));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user-alpha", result!.DfeSignInRef);
    }

    [Fact]
    public async Task UserRepository_GetUserByAsync_WhenPredicateDoesNotMatch_ThenReturnsNull()
    {
        // Arrange
        var user = new UserEntity { DfeSignInRef = "existing-user", DateCreated = DateTime.UtcNow };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserByAsync(u => u.DfeSignInRef == "nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UserRepository_CreateUserBySignInRefAsync_WhenCreatingMultipleUsers_ThenCreatesUniqueUsers()
    {
        // Arrange & Act
        var user1 = await _repository.CreateUserBySignInRefAsync("user-one");
        var user2 = await _repository.CreateUserBySignInRefAsync("user-two");

        // Assert
        Assert.NotEqual(user1.Id, user2.Id);
        Assert.Equal("user-one", user1.DfeSignInRef);
        Assert.Equal("user-two", user2.DfeSignInRef);

        // Verify both are saved
        var count = await CountEntitiesAsync<UserEntity>();
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task UserRepository_GetUserBySignInRefAsync_WhenDifferentCasing_ThenHandlesCaseInsensitively()
    {
        // Arrange
        var user = new UserEntity
        {
            DfeSignInRef = "CaseSensitiveUser",
            DateCreated = DateTime.UtcNow
        };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var resultExact = await _repository.GetUserBySignInRefAsync("CaseSensitiveUser");
        var resultLower = await _repository.GetUserBySignInRefAsync("casesensitiveuser");

        // Assert
        Assert.NotNull(resultExact);
        // Note: SQL Server is case-insensitive by default, so this will also find the user
        Assert.NotNull(resultLower);
        Assert.Equal(user.Id, resultExact!.Id);
        Assert.Equal(user.Id, resultLower!.Id);
    }
}
