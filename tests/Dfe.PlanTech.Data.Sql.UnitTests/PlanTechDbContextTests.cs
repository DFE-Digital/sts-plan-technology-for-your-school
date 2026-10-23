using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Providers.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

    // The stamping in SaveChangesAsync is generic over IUserActionEntity, so every implementation
    // should be covered rather than trusting that one representative entity stands for the rest.
    [Theory]
    [InlineData(nameof(AnswerEntity))]
    [InlineData(nameof(EstablishmentRecommendationHistoryEntity))]
    [InlineData(nameof(GroupReadActivityEntity))]
    [InlineData(nameof(QuestionEntity))]
    [InlineData(nameof(RecommendationEntity))]
    [InlineData(nameof(ResponseEntity))]
    [InlineData(nameof(SubmissionEntity))]
    [InlineData(nameof(UserContentViewEntity))]
    public async Task SaveChangesAsync_WhenUserActionEntityAdded_ThenSetsUserActionId(
        string entityTypeName
    )
    {
        var expectedUserActionId = Guid.NewGuid();

        using var db = BuildPlanTechDbContext(
            $"{nameof(SaveChangesAsync_WhenUserActionEntityAdded_ThenSetsUserActionId)}_{entityTypeName}",
            new TestUserActionIdAccessor(expectedUserActionId)
        );

        var entity = CreateUserActionEntity(entityTypeName);

        // Cast to object so EF resolves the entity's runtime type, rather than binding to the
        // generic overload and looking for an entity type named IUserActionEntity
        db.Add((object)entity);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(expectedUserActionId, entity.UserActionId);
    }

    // Guards the mistake the generic stamping cannot catch: adding an entity that has a
    // userActionId column but does not implement IUserActionEntity, so it is never stamped.
    [Fact]
    public void Model_WhenEntityHasAUserActionIdProperty_ThenItImplementsIUserActionEntity()
    {
        using var db = BuildPlanTechDbContext(
            nameof(Model_WhenEntityHasAUserActionIdProperty_ThenItImplementsIUserActionEntity)
        );

        var entitiesMissingTheInterface = db
            .Model.GetEntityTypes()
            .Select(entityType => entityType.ClrType)
            .Where(clrType =>
                clrType.GetProperty(nameof(AnswerEntity.UserActionId)) is not null
                && !typeof(IUserActionEntity).IsAssignableFrom(clrType)
            )
            .Select(clrType => clrType.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        Assert.Empty(entitiesMissingTheInterface);
    }

    private static IUserActionEntity CreateUserActionEntity(string entityTypeName) =>
        entityTypeName switch
        {
            nameof(AnswerEntity) => new AnswerEntity
            {
                AnswerText = "Answer",
                ContentfulRef = "A001",
            },
            nameof(EstablishmentRecommendationHistoryEntity) =>
                new EstablishmentRecommendationHistoryEntity
                {
                    EstablishmentId = 1,
                    RecommendationId = 1,
                    UserId = 1,
                    NewStatus = RecommendationStatus.NotStarted,
                },
            nameof(GroupReadActivityEntity) => new GroupReadActivityEntity
            {
                UserId = 1,
                UserEstablishmentId = 1,
                SelectedEstablishmentId = 1,
            },
            nameof(QuestionEntity) => new QuestionEntity
            {
                QuestionText = "Question",
                ContentfulRef = "Q001",
            },
            nameof(RecommendationEntity) => new RecommendationEntity
            {
                ContentfulRef = "R001",
                RecommendationText = "Recommendation",
                QuestionId = 1,
            },
            nameof(ResponseEntity) => new ResponseEntity
            {
                UserId = 1,
                SubmissionId = 1,
                QuestionId = 1,
                AnswerId = 1,
            },
            nameof(SubmissionEntity) => new SubmissionEntity
            {
                EstablishmentId = 1,
                SectionId = "SEC001",
                SectionName = "Section 001",
                Status = SubmissionStatus.InProgress,
            },
            nameof(UserContentViewEntity) => new UserContentViewEntity
            {
                UserId = 1,
                ContentfulRef = "C001",
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(entityTypeName),
                $"No factory is defined for '{entityTypeName}'. Add one when introducing a new IUserActionEntity."
            ),
        };
}
