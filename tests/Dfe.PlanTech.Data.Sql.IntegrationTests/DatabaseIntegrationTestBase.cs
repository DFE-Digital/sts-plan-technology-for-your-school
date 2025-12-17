using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests;

/// <summary>
/// Base class for integration tests that provides transaction isolation.
/// Each test (test method) gets a fresh DbContext wrapped in a transaction that is rolled back after the test.
/// This ensures tests don't interfere with each other while being fast and efficient.
/// </summary>
[Collection("Database collection")]
[Trait("Category", "Integration")]
public abstract class DatabaseIntegrationTestBase : IAsyncLifetime
{
    protected readonly DatabaseFixture Fixture;
    protected PlanTechDbContext DbContext { get; private set; } = null!;
    private IDbContextTransaction _transaction = null!;

    protected DatabaseIntegrationTestBase(DatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    public virtual async Task InitializeAsync()
    {
        // Create a fresh DbContext for this test to ensure complete isolation
        DbContext = Fixture.CreateDbContext();

        // Start a transaction that will be rolled back after the test
        // This provides data isolation while allowing SaveChanges() to work normally
        _transaction = await DbContext.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        // Roll back the transaction to clean up any changes made during the test
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await DbContext.DisposeAsync();
    }

    /// <summary>
    /// Helper method to count entities of a specific type within the current transaction.
    /// Useful for verifying CRUD operations without relying on specific ID values.
    /// </summary>
    protected async Task<int> CountEntitiesAsync<T>() where T : class
    {
        return await DbContext.Set<T>().CountAsync();
    }
}
