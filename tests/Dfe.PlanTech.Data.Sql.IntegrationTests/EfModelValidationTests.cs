using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests;

public class EfModelValidationTests : DatabaseIntegrationTestBase
{
    public EfModelValidationTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task EfModelValidationTests_EntityFrameworkModel_WhenPerformingCrudOperationsOnAllCoreEntities_ThenCanNavigateRelationships()
    {
        // Arrange: create and save all core entities
        var question = new QuestionEntity { QuestionText = "Q1", ContentfulRef = "Q1Ref" };
        var establishment = new EstablishmentEntity { EstablishmentRef = "E2", OrgName = "School 2" };
        var user = new UserEntity { DfeSignInRef = "U2" };

        DbContext.Questions.Add(question);
        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Ensure we have the generated IDs
        var questionId = question.Id;
        var establishmentId = establishment.Id;
        var userId = user.Id;

        // Verify the IDs were generated
        Assert.True(questionId > 0, "Question ID should be generated");
        Assert.True(establishmentId > 0, "Establishment ID should be generated");
        Assert.True(userId > 0, "User ID should be generated");

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Rec2",
            ContentfulRef = "R2",
            QuestionId = questionId,
            Archived = false
        };
        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var recommendationId = recommendation.Id;
        Assert.True(recommendationId > 0, "Recommendation ID should be generated");

        var history = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = establishmentId,
            RecommendationId = recommendationId,
            UserId = userId,
            NewStatus = "Completed",
            NoteText = "Test note"
        };
        DbContext.EstablishmentRecommendationHistories.Add(history);
        await DbContext.SaveChangesAsync();

        // Act: retrieve and assert navigation properties
        var loadedHistory = await DbContext.EstablishmentRecommendationHistories
            .Include(h => h.Establishment)
            .Include(h => h.Recommendation)
            .Include(h => h.User)
            .FirstOrDefaultAsync(h => h.EstablishmentId == establishmentId && h.RecommendationId == recommendationId);

        Assert.NotNull(loadedHistory);
        Assert.Equal("School 2", loadedHistory.Establishment.OrgName);
        Assert.Equal("Rec2", loadedHistory.Recommendation.RecommendationText);
        Assert.Equal("U2", loadedHistory.User.DfeSignInRef);
        Assert.Equal("Completed", loadedHistory.NewStatus);
        Assert.Equal("Test note", loadedHistory.NoteText);

        // Update
        loadedHistory.NoteText = "Updated note";
        await DbContext.SaveChangesAsync();
        var updated = await DbContext.EstablishmentRecommendationHistories.FindAsync(loadedHistory.Id);
        Assert.Equal("Updated note", updated!.NoteText);

        // Delete
        DbContext.EstablishmentRecommendationHistories.Remove(loadedHistory);
        await DbContext.SaveChangesAsync();
        var deleted = await DbContext.EstablishmentRecommendationHistories.FindAsync(loadedHistory.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task EfModelValidationTests_DatabaseConstraints_WhenInsertingInvalidForeignKey_ThenCannotInsertInvalidForeignKey()
    {
        // Try to insert a history with invalid foreign key IDs
        var invalidHistory = new EstablishmentRecommendationHistoryEntity
        {
            EstablishmentId = 99999,
            RecommendationId = 99999,
            UserId = 99999,
            NewStatus = "Invalid"
        };
        DbContext.EstablishmentRecommendationHistories.Add(invalidHistory);
        await Assert.ThrowsAsync<DbUpdateException>(() => DbContext.SaveChangesAsync());
    }
}
