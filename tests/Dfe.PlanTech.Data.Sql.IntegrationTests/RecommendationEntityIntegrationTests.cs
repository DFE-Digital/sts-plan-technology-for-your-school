using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests;

public class RecommendationEntityIntegrationTests : IntegrationTestBase
{
    public RecommendationEntityIntegrationTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task RecommendationEntities_DatabaseOperations_WhenInsertingAndRetrievingRecommendationAndHistory_ThenCanPerformCrudOperationsAndNavigateRelationships()
    {
        // Arrange: create required entities
        var question = new QuestionEntity { QuestionText = "Test Q", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "R1",
            QuestionId = question.Id,
            Archived = false
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var establishment = new EstablishmentEntity { EstablishmentRef = "E1", OrgName = "Test School" };
        var user = new UserEntity { DfeSignInRef = "U1" };
        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishment.Id,
            RecommendationId = recommendation.Id,
            UserId = user.Id,
            NewStatus = "Started"
        };
        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync();

        // Act: retrieve and assert
        var loaded = await DbContext.EstablishmentRecommendationHistories
            .Include(h => h.Establishment)
            .Include(h => h.Recommendation)
            .Include(h => h.User)
            .FirstOrDefaultAsync();

        Assert.NotNull(loaded);
        Assert.Equal(establishment.Id, loaded!.EstablishmentId);
        Assert.Equal(recommendation.Id, loaded.RecommendationId);
        Assert.Equal(user.Id, loaded.UserId);
        Assert.Equal("Started", loaded.NewStatus);
        Assert.Equal("Test School", loaded.Establishment.OrgName);
        Assert.Equal("Test Recommendation", loaded.Recommendation.RecommendationText);
        Assert.Equal("U1", loaded.User.DfeSignInRef);
    }
}
