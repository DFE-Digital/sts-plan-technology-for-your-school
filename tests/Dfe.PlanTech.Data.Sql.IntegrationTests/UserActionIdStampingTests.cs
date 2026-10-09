using Dfe.PlanTech.Core.Providers.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests;

/// <summary>
/// PlanTechDbContextTests covers the stamping against the in-memory provider. These tests confirm
/// the userActionId actually reaches a real database column, which no other integration test can
/// do: DatabaseIntegrationTestBase builds its context without a user action id provider, so the
/// stamping is a no-op there. Each test owns its context and cleans up after itself.
/// </summary>
[Collection("Database collection")]
[Trait("Category", "Integration")]
public class UserActionIdStampingTests(DatabaseFixture fixture)
{
    private const string StampedRef = "UAI-stamped-ref";
    private const string UnstampedRef = "UAI-unstamped-ref";

    [Fact]
    public async Task SaveChangesAsync_WhenProviderIsSupplied_ThenStampsUserActionIdInTheDatabase()
    {
        var expectedUserActionId = Guid.NewGuid();

        await using var dbContext = fixture.CreateDbContext(
            new TestUserActionIdProvider(expectedUserActionId)
        );

        try
        {
            dbContext.Questions.Add(
                new QuestionEntity { ContentfulRef = StampedRef, QuestionText = "Stamped" }
            );
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Read back through a separate context so the assertion reflects the stored column
            await using var verificationContext = fixture.CreateDbContext();
            var persisted = await verificationContext.Questions.SingleAsync(
                question => question.ContentfulRef == StampedRef,
                TestContext.Current.CancellationToken
            );

            Assert.Equal(expectedUserActionId, persisted.UserActionId);
        }
        finally
        {
            await CleanUpAsync(StampedRef);
        }
    }

    [Fact]
    public async Task SaveChangesAsync_WhenNoProviderIsSupplied_ThenLeavesUserActionIdNull()
    {
        await using var dbContext = fixture.CreateDbContext();

        try
        {
            dbContext.Questions.Add(
                new QuestionEntity { ContentfulRef = UnstampedRef, QuestionText = "Unstamped" }
            );
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            await using var verificationContext = fixture.CreateDbContext();
            var persisted = await verificationContext.Questions.SingleAsync(
                question => question.ContentfulRef == UnstampedRef,
                TestContext.Current.CancellationToken
            );

            Assert.Null(persisted.UserActionId);
        }
        finally
        {
            await CleanUpAsync(UnstampedRef);
        }
    }

    private async Task CleanUpAsync(string contentfulRef)
    {
        // No ambient transaction rolls these tests back, so the rows are removed here
        await using var cleanupContext = fixture.CreateDbContext();
        await cleanupContext
            .Questions.Where(question => question.ContentfulRef == contentfulRef)
            .ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }
}

internal class TestUserActionIdProvider(Guid userActionId) : IUserActionIdProvider
{
    public Guid GetUserActionId() => userActionId;
}
