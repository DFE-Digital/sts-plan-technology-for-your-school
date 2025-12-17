using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class SignInRepositoryTests : DatabaseIntegrationTestBase
{
    private SignInRepository _repository = null!;

    public SignInRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new SignInRepository(DbContext);
    }

    [Fact]
    public async Task SignInRepository_CreateSignInAsync_WhenGivenUserIdAndEstablishmentId_ThenCreatesSignInWithEstablishmentId()
    {
        // Arrange
        var user = new UserEntity { DfeSignInRef = "user123" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };

        DbContext.Users.Add(user);
        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync();

        var beforeSignIn = DateTime.UtcNow;

        // Act
        var result = await _repository.CreateSignInAsync(user.Id, establishment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(establishment.Id, result.EstablishmentId);
        Assert.True(result.SignInDateTime >= beforeSignIn);
        Assert.True(result.SignInDateTime <= DateTime.UtcNow);

        // Verify it was saved to database
        var saved = await DbContext.SignIns.FindAsync(result.Id);
        Assert.NotNull(saved);
        Assert.Equal(user.Id, saved!.UserId);
    }

    [Fact]
    public async Task SignInRepository_CreateSignInAsync_WhenGivenUserIdOnly_ThenCreatesSignInWithoutEstablishmentId()
    {
        // Arrange
        var user = new UserEntity { DfeSignInRef = "user456" };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var beforeSignIn = DateTime.UtcNow;

        // Act
        var result = await _repository.CreateSignInAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(user.Id, result.UserId);
        Assert.Null(result.EstablishmentId);
        Assert.True(result.SignInDateTime >= beforeSignIn);
        Assert.True(result.SignInDateTime <= DateTime.UtcNow);
    }

    [Fact]
    public async Task SignInRepository_CreateSignInAsync_WhenUserIdIsZero_ThenThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.CreateSignInAsync(0));

        Assert.Contains("userId", exception.Message);
        Assert.Contains("cannot be 0", exception.Message);
    }

    [Fact]
    public async Task SignInRepository_RecordSignInWithoutEstablishmentIdAsync_WhenUserExists_ThenCreatesSignInForExistingUser()
    {
        // Arrange
        var user = new UserEntity { DfeSignInRef = "existing-user" };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var beforeSignIn = DateTime.UtcNow;

        // Act
        var result = await _repository.RecordSignInWithoutEstablishmentIdAsync("existing-user");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(user.Id, result.UserId);
        Assert.Null(result.EstablishmentId);
        Assert.True(result.SignInDateTime >= beforeSignIn);
        Assert.True(result.SignInDateTime <= DateTime.UtcNow);

        // Verify it was saved to database
        var saved = await DbContext.SignIns.FindAsync(result.Id);
        Assert.NotNull(saved);
        Assert.Equal(user.Id, saved!.UserId);
    }

    [Fact]
    public async Task SignInRepository_RecordSignInWithoutEstablishmentIdAsync_WhenUserDoesNotExist_ThenThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.RecordSignInWithoutEstablishmentIdAsync("nonexistent-user"));

        Assert.Contains("Could not find user", exception.Message);
        Assert.Contains("nonexistent-user", exception.Message);
    }

    [Fact]
    public async Task SignInRepository_CreateSignInAsync_WhenCalledMultipleTimesForSameUser_ThenHandlesMultipleSignIns()
    {
        // Arrange
        var user = new UserEntity { DfeSignInRef = "frequent-user" };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act - Create multiple sign-ins
        var signIn1 = await _repository.CreateSignInAsync(user.Id);
        var signIn2 = await _repository.CreateSignInAsync(user.Id);

        // Assert
        Assert.NotEqual(signIn1.Id, signIn2.Id);
        Assert.Equal(user.Id, signIn1.UserId);
        Assert.Equal(user.Id, signIn2.UserId);
        Assert.True(signIn2.SignInDateTime >= signIn1.SignInDateTime);

        // Verify both are saved
        var count = await CountEntitiesAsync<SignInEntity>();
        Assert.Equal(2, count);
    }
}
