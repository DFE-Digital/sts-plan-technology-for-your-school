using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests;

/// <summary>
/// These tests deliberately do not derive from DatabaseIntegrationTestBase. That base class opens a
/// transaction per test and rolls it back on dispose, and TransactionManager cannot begin a
/// transaction inside an existing one. Each test here therefore owns its DbContext and cleans up
/// after itself.
/// </summary>
[Collection("Database collection")]
[Trait("Category", "Integration")]
public class TransactionManagerTests(DatabaseFixture fixture)
{
    private const string RolledBackRef = "TM-rolled-back-ref";
    private const string CommittedRef = "TM-committed-ref";

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenOperationThrows_ThenRollsBackTheWrites()
    {
        await using var dbContext = fixture.CreateDbContext();
        var sut = new TransactionManager(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.ExecuteInTransactionAsync(
                async () =>
                {
                    dbContext.Questions.Add(
                        new QuestionEntity
                        {
                            ContentfulRef = RolledBackRef,
                            QuestionText = "Rolled back",
                        }
                    );
                    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

                    throw new InvalidOperationException("Fail after the write has been saved");
                },
                TestContext.Current.CancellationToken
            )
        );

        // Verify through a separate context, so the assertion reflects the database rather than the
        // original context's change tracker
        await using var verificationContext = fixture.CreateDbContext();
        var persistedCount = await verificationContext.Questions.CountAsync(
            question => question.ContentfulRef == RolledBackRef,
            TestContext.Current.CancellationToken
        );

        Assert.Equal(0, persistedCount);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenOperationSucceeds_ThenCommitsTheWrites()
    {
        await using var dbContext = fixture.CreateDbContext();
        var sut = new TransactionManager(dbContext);

        try
        {
            await sut.ExecuteInTransactionAsync(
                async () =>
                {
                    dbContext.Questions.Add(
                        new QuestionEntity
                        {
                            ContentfulRef = CommittedRef,
                            QuestionText = "Committed",
                        }
                    );
                    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
                },
                TestContext.Current.CancellationToken
            );

            await using var verificationContext = fixture.CreateDbContext();
            var persistedCount = await verificationContext.Questions.CountAsync(
                question => question.ContentfulRef == CommittedRef,
                TestContext.Current.CancellationToken
            );

            Assert.Equal(1, persistedCount);
        }
        finally
        {
            // No ambient transaction rolls this test back, so the committed row is removed here
            await using var cleanupContext = fixture.CreateDbContext();
            await cleanupContext
                .Questions.Where(question => question.ContentfulRef == CommittedRef)
                .ExecuteDeleteAsync(TestContext.Current.CancellationToken);
        }
    }
}
