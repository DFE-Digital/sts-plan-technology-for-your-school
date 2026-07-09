using Dfe.PlanTech.Core.Providers.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.UnitTests;

public class PlanTechDbContextTests
{
    private class TestUserActionIdAccessor(Guid userActionId) : IUserActionIdProvider
    {
        public Guid GetUserActionId() => userActionId;
    }

    private static PlanTechDbContext BuildPlanTechDbContext(
        string name,
        IUserActionIdProvider? userActionIdProvider = null
    )
    {
        var options = new DbContextOptionsBuilder<PlanTechDbContext>()
            .UseInMemoryDatabase(name)
            .EnableSensitiveDataLogging()
            .Options;

        return new PlanTechDbContext(options, userActionIdProvider);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenEntityAdded_ThenSetsUserActionId()
    {
        var expectedUserActionId = Guid.NewGuid();

        using var db = BuildPlanTechDbContext(
            nameof(SaveChangesAsync_WhenEntityAdded_ThenSetsUserActionId),
            new TestUserActionIdAccessor(expectedUserActionId)
        );

        var answer = new AnswerEntity { AnswerText = "Answer", ContentfulRef = "A001" };

        db.Answers.Add(answer);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(expectedUserActionId, answer.UserActionId);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenEntityModified_ThenSetsUserActionId()
    {
        var updatedUserActionId = Guid.NewGuid();

        using var db = BuildPlanTechDbContext(
            nameof(SaveChangesAsync_WhenEntityModified_ThenSetsUserActionId),
            new TestUserActionIdAccessor(updatedUserActionId)
        );

        var answer = new AnswerEntity { AnswerText = "Answer", ContentfulRef = "A001" };

        db.Answers.Add(answer);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        db.Entry(answer).State = EntityState.Modified;

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(updatedUserActionId, answer.UserActionId);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAccessorIsNull_ThenDoesNotSetUserActionId()
    {
        using var db = BuildPlanTechDbContext(
            nameof(SaveChangesAsync_WhenAccessorIsNull_ThenDoesNotSetUserActionId)
        );

        var answer = new AnswerEntity { AnswerText = "Answer", ContentfulRef = "A001" };

        db.Answers.Add(answer);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Null(answer.UserActionId);
    }
}
