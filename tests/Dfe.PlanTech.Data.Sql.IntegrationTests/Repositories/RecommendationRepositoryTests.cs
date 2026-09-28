using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class RecommendationRepositoryTests : DatabaseIntegrationTestBase
{
    private RecommendationRepository _repository = null!;

    public RecommendationRepositoryTests(DatabaseFixture fixture)
        : base(fixture) { }

    public override async ValueTask InitializeAsync()
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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation1 = new RecommendationEntity
        {
            RecommendationText = "First Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };

        var recommendation2 = new RecommendationEntity
        {
            RecommendationText = "Second Recommendation",
            ContentfulRef = "rec-002",
            QuestionId = question.Id,
        };

        var recommendation3 = new RecommendationEntity
        {
            RecommendationText = "Third Recommendation",
            ContentfulRef = "rec-003",
            QuestionId = question.Id,
        };

        DbContext.Recommendations.AddRange(recommendation1, recommendation2, recommendation3);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var targetReferences = new[] { "rec-001", "rec-003" };

        // Act
        var result = await _repository.GetRecommendationsByContentfulReferencesAsync(
            targetReferences
        );

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var nonMatchingReferences = new[] { "rec-999", "rec-888" };

        // Act
        var result = await _repository.GetRecommendationsByContentfulReferencesAsync(
            nonMatchingReferences
        );

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendationsByContentfulReferencesAsync_WhenEmptyReferences_ThenReturnsEmpty()
    {
        // Arrange - Create a recommendation but search with empty reference array
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var emptyReferences = new string[0];

        // Act
        var result = await _repository.GetRecommendationsByContentfulReferencesAsync(
            emptyReferences
        );

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendationsByContentfulReferencesAsync_WhenDuplicateReferences_ThenReturnsDistinctResults()
    {
        // Arrange - Create one recommendation and search with duplicate references to test deduplication
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var recommendation = new RecommendationEntity
        {
            RecommendationText = "Test Recommendation",
            ContentfulRef = "rec-001",
            QuestionId = question.Id,
        };

        DbContext.Recommendations.Add(recommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var duplicateReferences = new[] { "rec-001", "rec-001", "rec-001" };

        // Act
        var result = await _repository.GetRecommendationsByContentfulReferencesAsync(
            duplicateReferences
        );

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
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var activeRecommendation = new RecommendationEntity
        {
            RecommendationText = "Active Recommendation",
            ContentfulRef = "rec-active",
            QuestionId = question.Id,
            Archived = false,
        };

        var archivedRecommendation = new RecommendationEntity
        {
            RecommendationText = "Archived Recommendation",
            ContentfulRef = "rec-archived",
            QuestionId = question.Id,
            Archived = true,
        };

        DbContext.Recommendations.AddRange(activeRecommendation, archivedRecommendation);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var references = new[] { "rec-active", "rec-archived" };

        // Act
        var result = await _repository.GetRecommendationsByContentfulReferencesAsync(references);

        // Assert
        var recommendations = result.ToList();
        Assert.Equal(2, recommendations.Count);
        Assert.Contains(recommendations, r => r.ContentfulRef == "rec-active" && !r.Archived);
        Assert.Contains(recommendations, r => r.ContentfulRef == "rec-archived" && r.Archived);
    }

    [Fact]
    public async Task UpsertRecommendations_WhenRecommendationIsNew_ThenInsertsAndReturnsIt()
    {
        // Arrange - No recommendation exists for the reference being upserted
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new SqlRecommendationDto
        {
            ContentfulSysId = "rec-new",
            RecommendationText = "Brand new recommendation",
            QuestionId = question.Id,
            QuestionContentfulRef = question.ContentfulRef,
        };

        // Act
        var result = await _repository.UpsertRecommendations([dto]);

        // Assert
        var inserted = Assert.Single(result);
        Assert.Equal("rec-new", inserted.ContentfulRef);
        Assert.Equal("Brand new recommendation", inserted.RecommendationText);
        Assert.Equal(question.Id, inserted.QuestionId);

        var persisted = await DbContext
            .Recommendations.Where(r => r.ContentfulRef == "rec-new")
            .ToListAsync(TestContext.Current.CancellationToken);
        Assert.Single(persisted);
    }

    [Fact]
    public async Task UpsertRecommendations_WhenRecommendationTextUnchanged_ThenDoesNotInsertAnotherRow()
    {
        // Arrange - An identical recommendation already exists, so the upsert should be a no-op
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var existing = new RecommendationEntity
        {
            RecommendationText = "Unchanged text",
            ContentfulRef = "rec-existing",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(existing);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new SqlRecommendationDto
        {
            ContentfulSysId = "rec-existing",
            RecommendationText = "Unchanged text",
            QuestionId = question.Id,
            QuestionContentfulRef = question.ContentfulRef,
        };

        // Act
        var result = await _repository.UpsertRecommendations([dto]);

        // Assert
        Assert.Single(result);

        var persisted = await DbContext
            .Recommendations.Where(r => r.ContentfulRef == "rec-existing")
            .ToListAsync(TestContext.Current.CancellationToken);
        Assert.Single(persisted);
    }

    [Fact]
    public async Task UpsertRecommendations_WhenRecommendationTextChanged_ThenInsertsANewVersion()
    {
        // Arrange - The recommendation text has changed, so a new version row should be written
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var existing = new RecommendationEntity
        {
            RecommendationText = "Original text",
            ContentfulRef = "rec-versioned",
            QuestionId = question.Id,
        };
        DbContext.Recommendations.Add(existing);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new SqlRecommendationDto
        {
            ContentfulSysId = "rec-versioned",
            RecommendationText = "Revised text",
            QuestionId = question.Id,
            QuestionContentfulRef = question.ContentfulRef,
        };

        // Act
        await _repository.UpsertRecommendations([dto]);

        // Assert
        var persisted = await DbContext
            .Recommendations.Where(r => r.ContentfulRef == "rec-versioned")
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, persisted.Count);
        Assert.Contains(persisted, r => r.RecommendationText == "Original text");
        Assert.Contains(persisted, r => r.RecommendationText == "Revised text");
    }
}
