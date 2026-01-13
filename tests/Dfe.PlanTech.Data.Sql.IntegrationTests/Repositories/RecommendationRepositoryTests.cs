using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class RecommendationRepositoryTests : DatabaseIntegrationTestBase
{
    private RecommendationRepository _repository = null!;

    public RecommendationRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new RecommendationRepository(DbContext);
    }

    [Fact]
    public async Task GetRecommendationsByContentfulReferencesAsync_WhenGivenValidReferences_ThenReturnsMatchingRecommendations()
    {
        // Arrange - Create multiple recommendations with different ContentfulRef values and search for specific ones
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation1 = new RecommendationEntity
        {
            RecommendationText = "First Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id
        };

        var recommendation2 = new RecommendationEntity
        {
            RecommendationText = "Second Recommendation",
            ContentfulRef = "rec-002",
            QuestionId = question.Id
        };

        var recommendation3 = new RecommendationEntity
        {
            RecommendationText = "Third Recommendation",
            ContentfulRef = "rec-003",
            QuestionId = question.Id
        };

        DbContext.Recommendations.AddRange(recommendation1, recommendation2, recommendation3);
        await DbContext.SaveChangesAsync();

        var targetReferences = new[] { "rec-001", "rec-003" };

        // Act
        var result = await _repository.GetRecommendationsByContentfulReferencesAsync(targetReferences);

        // Assert
        var recommendations = result.ToList();
        Assert.Equal(2, recommendations.Count);
        Assert.Contains(recommendations, r => r.ContentfulRef == "rec-001");
        Assert.Contains(recommendations, r => r.ContentfulRef == "rec-003");
        Assert.DoesNotContain(recommendations, r => r.ContentfulRef == "rec-002");
    }

    [Fact]
    public async Task GetRecommendationsByContentfulReferencesAsync_WhenNoMatchingReferences_ThenReturnsEmpty()
    {
        // Arrange - Create a recommendation with specific ContentfulRef and search for non-matching references
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var nonMatchingReferences = new[] { "rec-999", "rec-888" };

        // Act
        var result = await _repository.GetRecommendationsByContentfulReferencesAsync(nonMatchingReferences);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendationsByContentfulReferencesAsync_WhenEmptyReferences_ThenReturnsEmpty()
    {
        // Arrange - Create a recommendation but search with empty reference array
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var emptyReferences = new string[0];

        // Act
        var result = await _repository.GetRecommendationsByContentfulReferencesAsync(emptyReferences);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendationsByContentfulReferencesAsync_WhenDuplicateReferences_ThenReturnsDistinctResults()
    {
        // Arrange - Create one recommendation and search with duplicate references to test deduplication
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync();

        var duplicateReferences = new[] { "rec-001", "rec-001", "rec-001" };

        // Act
        var result = await _repository.GetRecommendationsByContentfulReferencesAsync(duplicateReferences);

        // Assert
        var recommendations = result.ToList();
        Assert.Single(recommendations);
        Assert.Equal("rec-001", recommendations.First().ContentfulRef);
    }

    [Fact]
    public async Task GetRecommendationsByContentfulReferencesAsync_WhenIncludesArchivedRecommendations_ThenReturnsAllMatches()
    {
        // Arrange - Create both active and archived recommendations to test that both are returned
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var activeRecommendation = new RecommendationEntity
        {
            RecommendationText = "Active Recommendation",
            ContentfulRef = "rec-active",
            QuestionId = question.Id,
            Archived = false
        };

        var archivedRecommendation = new RecommendationEntity
        {
            RecommendationText = "Archived Recommendation",
            ContentfulRef = "rec-archived",
            QuestionId = question.Id,
            Archived = true
        };

        DbContext.Recommendations.AddRange(activeRecommendation, archivedRecommendation);
        await DbContext.SaveChangesAsync();

        var references = new[] { "rec-active", "rec-archived" };

        // Act
        var result = await _repository.GetRecommendationsByContentfulReferencesAsync(references);

        // Assert
        var recommendations = result.ToList();
        Assert.Equal(2, recommendations.Count);
        Assert.Contains(recommendations, r => r.ContentfulRef == "rec-active" && !r.Archived);
        Assert.Contains(recommendations, r => r.ContentfulRef == "rec-archived" && r.Archived);
    }
}
